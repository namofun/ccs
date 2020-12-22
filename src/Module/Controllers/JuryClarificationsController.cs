using Ccs;
using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/jury/[controller]")]
    [AuditPoint(Entities.AuditlogType.Clarification)]
    public class ClarificationsController : JuryControllerBase
    {
        IClarificationStore Store { get; }

        public ClarificationsController(IClarificationStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(new JuryListClarificationModel
            {
                AllClarifications = await Store.ListAsync(Contest, c => c.Recipient == null),
                Problems = await Context.FetchProblemsAsync(),
                TeamNames = await Context.FetchTeamNamesAsync(),
                JuryName = User.GetUserName(),
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(AddClarificationModel model)
        {
            // validate the model
            if (string.IsNullOrWhiteSpace(model.Body))
                ModelState.AddModelError("xys::clar_empty", "Clarification body cannot be empty.");

            // reply clar
            Clarification replyTo = null;
            if (model.ReplyTo.HasValue)
            {
                replyTo = await Store.FindAsync(Contest, model.ReplyTo.Value);
                if (replyTo == null)
                    ModelState.AddModelError("xys::clar_not_found", "The clarification replied to not found.");
            }

            // determine category
            var problems = await Context.FetchProblemsAsync();
            var usage = problems.GetClarificationCategories().FirstOrDefault(cp => model.Type == cp.Item1);
            if (usage.Item1 == null)
                ModelState.AddModelError("xys::error_cate", "The category specified is wrong.");

            if (!ModelState.IsValid) return View(model);
            var clarId = await Store.SendAsync(
                replyTo: replyTo,
                clar: new Clarification
                {
                    Body = model.Body,
                    SubmitTime = DateTimeOffset.Now,
                    ContestId = Contest.Id,
                    JuryMember = User.GetUserName(),
                    Sender = null,
                    ResponseToId = model.ReplyTo,
                    Recipient = model.TeamTo == 0 ? default(int?) : model.TeamTo,
                    ProblemId = usage.Item3,
                    Answered = true,
                    Category = usage.Item2,
                });

            await HttpContext.AuditAsync("added", $"{clarId}");
            StatusMessage = $"Clarification {clarId} has been sent.";
            return RedirectToAction(nameof(Detail), new { clarid = clarId });
        }


        [HttpGet("[action]/{teamto}")]
        public IActionResult Send(int teamto)
        {
            return View(new AddClarificationModel { TeamTo = teamto, Body = "" });
        }


        [HttpGet("{clarid}/[action]/{answered}")]
        public async Task<IActionResult> SetAnswered(int clarid, bool answered)
        {
            var result = await Store.SetAnsweredAsync(Contest, clarid, answered);

            if (result && answered)
                return GoBackHome($"Clarification #{clarid} is now answered.", "List", "Clarifications");
            else if (result)
                return GoBackHome($"Clarification #{clarid} is now unanswered.", "Detail", "Clarifications");
            else
                return Message("Set clarification", "Unknown error.", BootstrapColor.danger);
        }


        [HttpGet("{clarid}")]
        public async Task<IActionResult> Detail(int clarid)
        {
            var clar = await Store.FindAsync(Contest, clarid);
            if (clar == null) return NotFound();
            var query = Enumerable.Repeat(clar, 1);

            if (clar.Sender == null && clar.ResponseToId != null)
            {
                var clar2 = await Store.FindAsync(Contest, clar.ResponseToId.Value);
                if (clar2 != null) query = query.Prepend(clar2);
            }
            else if (clar.Sender != null)
            {
                var otherClars = await Store.ListAsync(Contest, c => c.ResponseToId == clarid && c.Sender == null);
                query = query.Concat(otherClars);
            }

            return View(new JuryViewClarificationModel
            {
                Associated = query,
                Main = query.First(),
                Problems = await Context.FetchProblemsAsync(),
                Teams = await Context.FetchTeamNamesAsync(),
                UserName = User.GetUserName(),
            });
        }


        [HttpGet("{clarid}/[action]/{claim}")]
        public async Task<IActionResult> Claim(int cid, int clarid, bool claim)
        {
            var admin = User.GetUserName();
            var result = await Store.ClaimAsync(Contest, clarid, admin, claim);

            if (result && claim)
                return RedirectToAction(nameof(Detail));
            else if (result)
                return RedirectToAction(nameof(List));
            else
                return GoBackHome($"Clarification has been claimed before.", "List", "Clarifications");
        }
    }
}
