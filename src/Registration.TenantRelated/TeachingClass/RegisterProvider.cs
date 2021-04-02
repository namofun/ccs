using Ccs.Registration.TeachingClass;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using SatelliteSite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Services;

namespace Ccs.Registration
{
    public class TeachingClassRegisterProvider : JuryRegisterProviderBase<InputModel, OutputModel>
    {
        public override int Order => -500;

        public override string Name => "Teaching class";

        public override string Icon => "fas fa-graduation-cap";

        public override string FancyName => "student-by-class";

        protected override async Task<InputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            var studentStore = context.GetRequiredService<IStudentStore>();
            var userId = int.Parse(context.User.GetUserId()!);
            IEnumerable<int> tenantId = context.User.IsInRole("Administrator")
                ? context.GetRequiredService<IAffiliationStore>().GetQueryable().Select(a => a.Id)
                : context.User.FindAll("tenant_admin").Select(a => int.Parse(a.Value));

            return new InputModel
            {
                Classes = await studentStore.ListClassesAsync(tenantId, c => c.UserId == null || c.UserId == userId),
                Categories = await context.ListCategoriesAsync(false),
            };
        }

        protected override Task ValidateAsync(RegisterProviderContext context, InputModel model, ModelStateDictionary modelState)
        {
            if (!model.Categories.ContainsKey(model.CategoryId))
            {
                modelState.AddModelError("tcrp::nocat", "Category not found.");
            }

            if (model.Classes.Count(a => a.Id == model.ClassId) != 1)
            {
                modelState.AddModelError("tcrp::noaff", "Student group not found.");
            }

            return Task.CompletedTask;
        }

        protected override async Task<OutputModel> ExecuteAsync(RegisterProviderContext context, InputModel model)
        {
            var studentStore = context.GetRequiredService<IStudentStore>();
            var @class = model.Classes.Single(a => a.Id == model.ClassId);
            var category = model.Categories[model.CategoryId];
            int classId = model.ClassId, catId = model.CategoryId;
            int cid = context.Contest.Id, affId = @class.AffiliationId;

            var students = await studentStore.ListStudentsAsync(@class);
            var targets = students.ToLookup(
                keySelector: s => new { s.Id, s.Name },
                elementSelector: s => s.UserId.HasValue ? new { UserId = s.UserId!.Value, UserName = s.UserName! } : null);

            var _allTeams = await context.ListTeamsAsync(t => t.Status == 1);
            var allTeams = _allTeams.ToDictionary(a => a.TeamId);
            var existing = allTeams.Values
                .Where(t => t.AffiliationId == affId && t.CategoryId == catId)
                .Select(t => t.TeamName)
                .ToHashSet();

            var members = await context.GetTeamMembersAsync();
            var memberBelong = members
                .SelectMany(g => g, (g, c) => new { UserName = c, allTeams[g.Key].TeamName })
                .ToDictionary(a => a.UserName, a => a.TeamName);

            var result = new OutputModel
            {
                Existing = new List<string>(),
                NotEligible = new List<string>(),
                UserDuplicate = new List<(string, string, string)>(),
                Finished = new List<(int, string)>(),
            };

            foreach (var student in targets)
            {
                var teamName = student.Key.Id.Split('_')[1] + student.Key.Name;

                if (existing.Contains(teamName))
                {
                    result.Existing.Add(teamName);
                }
                else if (!student.Where(a => a != null).Any())
                {
                    result.NotEligible.Add(teamName);
                }
                else if (student.Any(a => memberBelong.ContainsKey(a!.UserName)))
                {
                    var err = student.Where(a => memberBelong.ContainsKey(a!.UserName)).First();
                    result.UserDuplicate.Add((teamName, err!.UserName, memberBelong[err.UserName]));
                }
                else
                {
                    var r = await context.CreateTeamAsync(
                        users: student.Select(a => new FakeRegisterUser(a!.UserId)), // no item will be null
                        team: new Entities.Team
                        {
                            AffiliationId = affId,
                            CategoryId = catId,
                            ContestId = cid,
                            Status = 1,
                            TeamName = teamName,
                        });

                    result.Finished.Add((r.TeamId, r.TeamName));
                }
            }

            return result;
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<InputModel> output)
        {
            output.WithTitle("Group register")
                .AppendValidationSummary()
                .AppendSelect(
                    @for: __model => __model.ClassId,
                    items: output.Model.Classes.Select(c => new SelectListItem(c.Name, $"{c.Id}")))
                .AppendSelect(
                    @for: __model => __model.CategoryId,
                    items: output.Model.Categories.Values.Select(c => new SelectListItem(c.Name, $"{c.Id}")));

            return Task.CompletedTask;
        }

        protected override Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<OutputModel> output)
        {
            output.WithTitle("Group register result")
                .AppendDataTable(
                    elements: output.Model.CreateResult(),
                    tableClass: "table-hover",
                    theadClass: "thead-light");

            return Task.CompletedTask;
        }
    }
}
