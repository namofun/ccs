using Ccs.Connector.PlagiarismDetect.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using SatelliteSite.ContestModule.Controllers;
using SatelliteSite.PlagModule.Models;
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


        [HttpGet("submissions/{submitid}")]
        public async Task<IActionResult> Submission(int submitid)
        {
            var vertex = await Service.GetComparisonsBySubmissionAsync(PlagiarismSet.Id, submitid);
            if (vertex == null) return NotFound();

            if (vertex.TokenProduced == false)
            {
                var er = await Service.GetCompilationAsync(PlagiarismSet.Id, submitid);
                ViewBag.Error = er.Error;
            }
            else
            {
                ViewBag.Error = null;
            }

            return View(vertex);
        }


        [HttpGet("reports/{rid}")]
        public async Task<IActionResult> Report(string rid)
        {
            var report = await Service.FindReportAsync(rid);
            if (report == null || report.SetId != PlagiarismSet.Id) return NotFound();

            var subA = await Service.FindSubmissionAsync(report.SetId, report.SubmissionA);
            var subB = await Service.FindSubmissionAsync(report.SetId, report.SubmissionB);

            var retA = CodeModel.CreateView(report, c => c.FileA, c => c.ContentStartA, c => c.ContentEndA, subA);
            var retB = CodeModel.CreateView(report, c => c.FileB, c => c.ContentStartB, c => c.ContentEndB, subB);

            return View(new ReportModel(report, retA, retB));
        }
    }
}
