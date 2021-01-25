using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Registration.BatchByTeamName
{
    public class BatchByTeamNameRegisterProvider : RegisterProviderBase<BatchByTeamNameInputModel, BatchByTeamNameOutputModel>
    {
        public override bool JuryOrContestant => true;

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

        protected override async Task<BatchByTeamNameInputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            return new BatchByTeamNameInputModel
            {
                Affiliations = await context.Context.FetchAffiliationsAsync(false),
                Categories = await context.Context.FetchCategoriesAsync(false),
            };
        }

        protected override async Task ValidateAsync(RegisterProviderContext context, BatchByTeamNameInputModel model, ModelStateDictionary modelState)
        {
            model.Affiliations ??= await context.Context.FetchAffiliationsAsync(false);
            model.Categories ??= await context.Context.FetchCategoriesAsync(false);

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
        }

        protected override async Task<BatchByTeamNameOutputModel> ExecuteAsync(RegisterProviderContext context, BatchByTeamNameInputModel model)
        {
            var rng = CreatePasswordGenerator();
            var result = new List<TeamAccount>();

            var teamNames = model.TeamNames.Split('\n');
            int affId = model.AffiliationId, catId = model.CategoryId;
            var existingTeams = await context.Context.ListTeamsAsync(c => c.AffiliationId == affId && c.CategoryId == catId);
            var list = existingTeams.ToLookup(a => a.TeamName);

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
                        ContestId = context.Context.Contest.Id,
                        Status = 1,
                        TeamName = item,
                    };

                    await context.Context.CreateTeamAsync(team, null);
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

            return new BatchByTeamNameOutputModel(result);

            async Task<IUser> EnsureTeamWithPassword(Team team, string password)
            {
                string username = UserNameForTeamId(team.TeamId);
                var UserManager = context.UserManager;
                var user = await UserManager.FindByNameAsync(username);

                if (user != null)
                {
                    if (await UserManager.HasPasswordAsync(user))
                    {
                        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                        await UserManager.ResetPasswordAsync(user, token, password);
                    }
                    else
                    {
                        await UserManager.AddPasswordAsync(user, password);
                    }

                    if (await UserManager.IsLockedOutAsync(user))
                    {
                        await UserManager.SetLockoutEndDateAsync(user, null);
                    }
                }
                else
                {
                    user = UserManager.CreateEmpty(username);
                    user.Email = $"{username}@contest.acm.xylab.fun";
                    await UserManager.CreateAsync(user, password);
                }

                await context.Context.AttachMemberAsync(team, user, true);
                return user;
            }
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<BatchByTeamNameInputModel> output)
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

        protected override Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<BatchByTeamNameOutputModel> output)
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
