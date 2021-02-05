using Ccs.Entities;
using Ccs.Registration.BatchByTeamName;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    public class BatchByTeamNameRegisterProvider : JuryRegisterProviderBase<InputModel, OutputModel>
    {
        public override int Order => -1000;

        public override string Name => "Temporary users";

        public override string Icon => "fas fa-envelope-open-text";

        private static string UserNameForTeamId(int teamId) => $"team{teamId:D3}";

        private static Func<string> CreatePasswordGenerator()
        {
            const string passwordSource = "abcdefhjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ2345678";
            var rng = new Random(unchecked((int)DateTimeOffset.Now.Ticks));
            return () =>
            {
                Span<char> pwd = stackalloc char[8];
                for (int i = 0; i < 8; i++) pwd[i] = passwordSource[rng.Next(passwordSource.Length)];
                return pwd.ToString();
            };
        }

        protected override async Task<InputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            return new InputModel
            {
                Affiliations = await context.FetchAffiliationsAsync(false),
                Categories = await context.FetchCategoriesAsync(false),
            };
        }

        protected override Task ValidateAsync(RegisterProviderContext context, InputModel model, ModelStateDictionary modelState)
        {
            if (!model.Categories.ContainsKey(model.CategoryId))
            {
                modelState.AddModelError("bbtnrp::nocat", "Category not found.");
            }

            if (!model.Affiliations.ContainsKey(model.AffiliationId))
            {
                modelState.AddModelError("bbtnrp::noaff", "Affiliation not found.");
            }

            if (string.IsNullOrWhiteSpace(model.TeamNames))
            {
                modelState.AddModelError("bbtnrp::notn", "No team name specified.");
            }

            return Task.CompletedTask;
        }

        protected override async Task<OutputModel> ExecuteAsync(RegisterProviderContext context, InputModel model)
        {
            var rng = CreatePasswordGenerator();
            var result = new List<TeamAccount>();

            var teamNames = model.TeamNames.Split('\n');
            int affId = model.AffiliationId, catId = model.CategoryId;
            var existingTeams = await context.ListTeamsAsync(c => c.Status == 1 && c.AffiliationId == affId && c.CategoryId == catId);
            var list = existingTeams.ToLookup(a => a.TeamName);
            var userManager = context.UserManager;

            foreach (var item2 in teamNames)
            {
                var item = item2.Trim();

                if (list.Contains(item))
                {
                    var e = list[item];
                    foreach (var team in e)
                    {
                        string pwd = rng();
                        var user = await EnsureTeamWithPassword(team, pwd);

                        result.Add(new TeamAccount
                        {
                            Id = team.TeamId,
                            TeamName = team.TeamName,
                            UserName = user.UserName,
                            Password = pwd,
                        });
                    }
                }
                else
                {
                    var team = new Team
                    {
                        AffiliationId = affId,
                        CategoryId = catId,
                        ContestId = context.Contest.Id,
                        Status = 1,
                        TeamName = item,
                    };

                    await context.CreateTeamAsync(team, null);
                    string pwd = rng();
                    var user = await EnsureTeamWithPassword(team, pwd);

                    result.Add(new TeamAccount
                    {
                        Id = team.TeamId,
                        TeamName = team.TeamName,
                        UserName = user.UserName,
                        Password = pwd,
                    });
                }
            }

            return new OutputModel(result);

            async Task<IUser> EnsureTeamWithPassword(Team team, string password)
            {
                string username = UserNameForTeamId(team.TeamId);
                var user = await userManager.FindByNameAsync(username);

                if (user != null)
                {
                    if (user.HasPassword())
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        await userManager.ResetPasswordAsync(user, token, password);
                    }
                    else
                    {
                        await userManager.AddPasswordAsync(user, password);
                    }

                    if (await userManager.IsLockedOutAsync(user))
                    {
                        await userManager.SetLockoutEndDateAsync(user, null);
                    }
                }
                else
                {
                    user = userManager.CreateEmpty(username);
                    user.Email = $"{username}@contest.acm.xylab.fun";
                    await userManager.CreateAsync(user, password);
                }

                await context.AttachMemberAsync(team, user, true);
                return user;
            }
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<InputModel> output)
        {
            output.WithTitle("Batch team register")
                .AppendValidationSummary()
                .AppendSelect(
                    @for: __model => __model.AffiliationId,
                    items: output.Model.Affiliations.Values.Select(a => new SelectListItem(a.Name, $"{a.Id}")))
                .AppendSelect(
                    @for: __model => __model.CategoryId,
                    items: output.Model.Categories.Values.Select(c => new SelectListItem(c.Name, $"{c.Id}")))
                .AppendTextArea(
                    @for: __model => __model.TeamNames,
                    comment: "队伍名称，每个一行，区分大小写和空格，提交后会绑定新用户并重置密码。");

            return Task.CompletedTask;
        }

        protected override Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<OutputModel> output)
        {
            output.WithTitle("Batch import result")
                .AppendDataTable(
                    elements: output.Model,
                    tableClass: "text-center table-hover",
                    theadClass: "thead-light");

            return Task.CompletedTask;
        }
    }
}
