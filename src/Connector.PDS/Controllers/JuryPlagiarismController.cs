using Ccs.Connector.PlagiarismDetect.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using SatelliteSite.ContestModule.Controllers;
using System.Threading.Tasks;

namespace Ccs.Connector.PlagiarismDetect.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(3)}/jury/plagiarism-detect")]
    public partial class JuryPlagiarismController : JuryControllerBase<IJuryContext>
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int PerPage = 50;
            var totalPages = (PlagiarismSet.SubmissionCount - 1) / PerPage + 1;
            if (page <= 0 || page > totalPages) return BadRequest();

            var subs = await Service.ListSubmissionsAsync(
                setid: PlagiarismSet.Id,
                skip: (page - 1) * PerPage,
                limit: PerPage,
                order: "percent",
                asc: false);

            return View(new IndexModel
            {
                PlagiarismSet = PlagiarismSet,
                Problems = await Context.ListProblemsAsync(),
                TeamNames = await Context.GetTeamNamesAsync(),
                Submissions = subs,
                CurrentPage = page,
                TotalPages = totalPages,
            });
        }


        [HttpGet("sync")]
        public async Task<IActionResult> Synchronize()
        {
            return Window(new SynchronizationOptionsModel
            {
                Problems = await Context.ListProblemsAsync(),
            });
        }


        [HttpPost("sync")]
        public IActionResult SynchronizeExecute(SynchronizationOptionsModel model)
        {
            return InAjax
                ? new SynchronizeResult(model, PlagiarismSet)
                : (IActionResult)View(model);
        }
    }
}
