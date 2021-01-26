using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    public partial class JuryTeamsController : JuryControllerBase
    {
        /*
        [HttpGet("[action]")]
        [Authorize(Roles = "Administrator,Teacher")]
        public async Task<IActionResult> ByGroup(int cid,
            [FromServices] IStudentStore store)
        {
            ViewBag.Aff = await Store.ListAffiliationAsync(cid, false);
            ViewBag.Cat = await Store.ListCategoryAsync(cid);
            ViewBag.Class = await store.ListClassAsync();

            return Window(new AddTeamByGroupModel
            {
                AffiliationId = 1,
                CategoryId = 3,
                AddNonTemporaryUser = true
            });
        }


        [HttpPost("[action]")]
        [Authorize(Roles = "Administrator,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ByGroup(AddTeamByGroupModel model)
        {
            var cls = await store.FindClassAsync(model.GroupId);
            if (cls == null) return NotFound();

            var affs = await Store.ListAffiliationAsync(cid, false);
            var cats = await Store.ListCategoryAsync(cid);
            var aff = affs.SingleOrDefault(a => a.AffiliationId == model.AffiliationId);
            var cat = cats.SingleOrDefault(c => c.CategoryId == model.CategoryId);
            if (aff == null || cat == null) return NotFound();

            var stus = await store.ListStudentsAsync(cls);
            var stu = stus.ToLookup(s => new { s.Id, s.Name });
            var uids = await Store.ListMemberUidsAsync(cid);

            foreach (var item in stu)
            {
                var lst = item.Where(s => s.IsVerified ?? false);
                if (model.AddNonTemporaryUser) lst = lst.Where(s => s.Email == null);
                var users = lst.Select(s => s.UserId.Value)
                    .Where(i => !uids.Contains(i))
                    .ToHashSet();

                await Store.CreateAsync(
                    uids: users.Count > 0 ? users.ToArray() : null,
                    team: new Team
                    {
                        AffiliationId = model.AffiliationId,
                        Status = 1,
                        CategoryId = model.CategoryId,
                        ContestId = Contest.ContestId,
                        TeamName = $"{item.Key.Id}{item.Key.Name}",
                    });
            }

            StatusMessage = "Import success.";
            return RedirectToAction(nameof(List));
        }
        */
    }
}
