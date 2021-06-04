using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/categories")]
    [AuditPoint(AuditlogType.Category)]
    public class JuryCategoriesController : JuryControllerBase<ITeamContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(new JuryListCategoriesModel
            {
                Categories = await Context.ListCategoriesAsync(false),
                TeamCount = await Context.AggregateTeamsAsync(t => t.CategoryId, t => t.Count()),
            });
        }


        [HttpGet("{catid}")]
        public async Task<IActionResult> Detail(int catid)
        {
            var cat = await Context.FindCategoryAsync(catid, false);
            if (cat == null) return NotFound();

            return View(new JuryViewCategoryModel
            {
                Category = cat,
                Teams = await Context.ListTeamsAsync(t => t.Status == 1 && t.CategoryId == catid),
            });
        }


        [HttpGet("[action]")]
        public IActionResult Add()
        {
            return View("Edit", new CategoryModel());
        }


        [HttpGet("{catid}/[action]")]
        public async Task<IActionResult> Edit(int catid)
        {
            var cat = await Context.FindCategoryAsync(catid, false);
            if (cat == null) return NotFound();
            if (!cat.ContestId.HasValue) return GoBackHome("Please edit the global category in dashboard.", "List", "JuryCategories");

            return View(new CategoryModel
            {
                Id = cat.Id,
                Color = cat.Color,
                SortOrder = cat.SortOrder,
                IsPublic = cat.IsPublic,
                Name = cat.Name,
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CategoryModel model)
        {
            var e = await Context.CreateCategoryAsync(new Category
            {
                Color = model.Color,
                SortOrder = model.SortOrder,
                IsPublic = model.IsPublic,
                Name = model.Name,
                ContestId = Contest.Id,
            });

            await HttpContext.AuditAsync("created", $"{e.Id}");
            return RedirectToAction(nameof(Detail), new { catid = e.Id });
        }


        [HttpPost("{catid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int catid, CategoryModel model)
        {
            var cat = await Context.FindCategoryAsync(catid, false);
            if (cat == null) return NotFound();
            if (!cat.ContestId.HasValue) return GoBackHome("Please edit the global category in dashboard.", "List", "JuryCategories");

            if (!ModelState.IsValid) return View(model);
            cat.Color = model.Color;
            cat.IsPublic = model.IsPublic;
            cat.Name = model.Name;
            cat.SortOrder = model.SortOrder;

            await Context.UpdateCategoryAsync(cat);
            await HttpContext.AuditAsync("updated", $"{catid}");
            return RedirectToAction(nameof(Detail), new { catid });
        }


        [HttpGet("{catid}/[action]")]
        public async Task<IActionResult> Delete(int catid)
        {
            var desc = await Context.FindCategoryAsync(catid, false);
            if (desc == null) return NotFound();

            if (desc.ContestId.HasValue)
            {
                return AskPost(
                    title: $"Delete team category {catid} - \"{desc.Name}\"",
                    message: $"You're about to delete team category {catid} - \"{desc.Name}\".\n" +
                        "Are you sure?",
                    type: BootstrapColor.danger);
            }
            else
            {
                return Message(
                    title: $"Delete team category {catid} - \"{desc.Name}\"",
                    message: "For global categories, please contact the administrator.",
                    type: BootstrapColor.danger);
            }
        }


        [HttpPost("{catid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int catid, int inajax)
        {
            var desc = await Context.FindCategoryAsync(catid, false);
            if (desc == null || !desc.ContestId.HasValue) return NotFound();

            try
            {
                await Context.DeleteCategoryAsync(desc);
                StatusMessage = $"Team category {catid} deleted successfully.";
                await HttpContext.AuditAsync("deleted", $"{catid}");
            }
            catch
            {
                StatusMessage = $"Error deleting team category {catid}, foreign key constraints failed.";
            }

            return RedirectToAction(nameof(List));
        }
    }
}
