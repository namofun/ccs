using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Services;
using SatelliteSite.ContestModule.Controllers;
using System.Threading.Tasks;

namespace Ccs.Connector.PlagiarismDetect.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(7)}/jury/plagiarism-detect")]
    [AffiliateTo(typeof(SatelliteSite.ContestModule.ContestModule<>))]
    public class JuryPlagiarismController : JuryControllerBase<IContestContext>
    {
        private IPlagiarismDetectService Service { get; }

        public JuryPlagiarismController(IPlagiarismDetectService pds)
        {
            Service = pds;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sets = await Service.ListSetsAsync(cid: Contest.Id);
            return View(sets);
        }
    }
}
