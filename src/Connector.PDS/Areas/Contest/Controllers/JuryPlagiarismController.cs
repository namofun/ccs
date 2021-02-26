using Ccs.Connector.PlagiarismDetect.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Plag.Backend.Models;
using Plag.Backend.Services;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.Threading.Tasks;

namespace Ccs.Connector.PlagiarismDetect.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(7)}/jury/plagiarism-detect")]
    [AffiliateTo(typeof(SatelliteSite.ContestModule.ContestModule<>))]
    public class JuryPlagiarismController : JuryControllerBase<IJuryContext>
    {
        private IPlagiarismDetectService Service { get; }

        public JuryPlagiarismController(IPlagiarismDetectService pds)
        {
            Service = pds;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var set = Contest.Settings.PlagiarismSet != null
                ? await Service.FindSetAsync(Contest.Settings.PlagiarismSet)
                : null;

            return set == null
                ? RedirectToAction(nameof(Link))
                : View(set) as IActionResult;
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Link()
        {
            var sets = await Service.ListSetsAsync(cid: Contest.Id);
            return View(sets);
        }


        [HttpGet("[action]/{setid}")]
        public async Task<IActionResult> Link(string setid)
        {
            var set = await Service.FindSetAsync(setid);
            if (set == null || set.ContestId != Contest.Id) return NotFound();

            return AskPost(
                title: "Link to plagiarism set",
                message: $"Are you sure to link to \"{set.Name}\" ({set.Id})?");
        }


        [HttpPost("[action]/{setid}")]
        public async Task<IActionResult> Link(string setid, bool post = true)
        {
            var set = await Service.FindSetAsync(setid);
            if (set == null || set.ContestId != Contest.Id) return NotFound();

            var settings = Contest.Settings.Clone();
            settings.PlagiarismSet = set.Id;
            await Context.UpdateSettingsAsync(settings);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet("[action]")]
        public IActionResult Create()
        {
            return Window(new CreateSetModel
            {
                Name = Contest.Name,
            });
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Create(CreateSetModel model)
        {
            var set = await Service.CreateSetAsync(
                new SetCreation
                {
                    ContestId = Contest.Id,
                    Name = model.Name,
                    UserId = int.Parse(User.GetUserId()),
                });

            var settings = Contest.Settings.Clone();
            settings.PlagiarismSet = set.Id;
            await Context.UpdateSettingsAsync(settings);
            return RedirectToAction(nameof(Index));
        }
    }
}
