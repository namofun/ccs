using Ccs.Registration.TeachingClass;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SatelliteSite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Services;

namespace Ccs.Registration
{
    public class TeachingClassRegisterProvider : RegisterProviderBase<InputModel, OutputModel>
    {
        public override int Order => -500;

        public override string Name => "Teaching class";

        public override string Icon => "fas fa-graduation-cap";

        public override bool JuryOrContestant => true;

        protected override async Task<InputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            var studentStore = context.GetRequiredService<IStudentStore>();
            IEnumerable<int> tenantId = context.User.IsInRole("Administrator")
                ? context.GetRequiredService<IAffiliationStore>().GetQueryable().Select(a => a.Id)
                : context.User.FindAll("tenant_admin").Select(a => int.Parse(a.Value));

            return new InputModel
            {
                Classes = await studentStore.ListClassesAsync(tenantId),
                Categories = await context.Context.FetchCategoriesAsync(false),
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
            int cid = context.Context.Contest.Id, affId = @class.AffiliationId;

            var db = studentStore.GetQueryableStore();
            var db2 = context.Context.GetQueryableStore();

            var query =
                from cs in db.ClassStudents
                where cs.ClassId == classId
                join s in db.Students on cs.StudentId equals s.Id
                join u in db.Users on s.Id equals u.StudentId into uu
                from u in uu.DefaultIfEmpty()
                select new { s, u };

            var targets = await query.ToLookupAsync(a => a.s, a => a.u);

            var query2 =
                from t in db2.Teams
                where t.ContestId == cid && t.Status == 1
                where t.AffiliationId == affId && t.CategoryId == catId
                select t.TeamName;

            var existing = await query2.ToHashSetAsync();

            var query3 =
                from t in db2.Teams
                where t.ContestId == cid && t.Status == 1
                join tm in db2.TeamMembers on new { t.ContestId, t.TeamId } equals new { tm.ContestId, tm.TeamId }
                select new { tm.UserId, t.TeamName };

            var memberBelong = await query3.ToDictionaryAsync(a => a.UserId, a => a.TeamName);

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
                else if (student.Any(a => memberBelong.ContainsKey(a.Id)))
                {
                    var err = student.Where(a => memberBelong.ContainsKey(a.Id)).First();
                    result.UserDuplicate.Add((teamName, err.UserName, memberBelong[err.Id]));
                }
                else
                {
                    var r = await context.Context.CreateTeamAsync(
                        users: student, // no item will be null
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
