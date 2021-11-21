using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/languages")]
    [AuditPoint(AuditlogType.Team)]
    public class JuryLanguagesController : JuryControllerBase<IJuryContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var models = await Context.ListLanguagesAsync(contestFiltered: false);
            return View(models);
        }


        [HttpGet("{langid}")]
        public async Task<IActionResult> Detail(string langid, bool all_submissions = false)
        {
            var lang = await Context.FindLanguageAsync(langid, contestFiltered: false);
            if (lang == null) return NotFound();

            var sols = await Context.ListSolutionsAsync(langid: langid, all: all_submissions);
            var tn = await Context.GetTeamNamesAsync();
            sols.ForEach(a => a.AuthorName = tn.GetValueOrDefault(a.TeamId));

            return View(new JuryViewLanguageModel
            {
                Language = lang,
                Solutions = sols,
            });
        }
    }
}
