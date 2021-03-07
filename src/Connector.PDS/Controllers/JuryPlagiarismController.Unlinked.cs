using Ccs.Connector.PlagiarismDetect.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Plag.Backend.Models;
using Plag.Backend.Services;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.Threading.Tasks;

namespace Ccs.Connector.PlagiarismDetect.Controllers
{
    public partial class JuryPlagiarismController : JuryControllerBase<IJuryContext>
    {
        private IPlagiarismDetectService Service { get; }

        private PlagiarismSet PlagiarismSet { get; set; }

        public JuryPlagiarismController(IPlagiarismDetectService pds)
        {
            Service = pds;
        }

        public override async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            await base.OnActionExecutingAsync(context);
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            if (actionDescriptor.ActionName != nameof(Link) && actionDescriptor.ActionName != nameof(Create))
            {
                PlagiarismSet = Contest.Settings.PlagiarismSet != null
                    ? await Service.FindSetAsync(Contest.Settings.PlagiarismSet)
                    : null;

                if (PlagiarismSet == null) context.Result = RedirectToAction(nameof(Link));
            }
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
