using Ccs;
using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for printing to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class PrintingController : Microsoft.AspNetCore.Mvc.ApiControllerBase
    {
        private static readonly AsyncLock _locker = new AsyncLock();
        private readonly IPrintingService _store;
        private readonly ScopedContestContextFactory _factory;
        private readonly IUserManager _userManager;

        public PrintingController(IPrintingService store, ScopedContestContextFactory factory, IUserManager userManager)
        {
            _factory = factory;
            _store = store;
            _userManager = userManager;
        }

        private async Task<PrintingDocument> SummarizeAsync(Printing p, bool done)
        {
            string room, name;

            var ctx = await _factory.CreateAsync(p.ContestId, false);
            var t = await ctx.FindTeamByUserAsync(p.UserId);
            if (t == null)
            {
                var u = await _userManager.FindByIdAsync(p.UserId);
                name = $"u{u.Id}: {u.UserName}";
                room = "Jury Room";
            }
            else
            {
                room = t.Location ?? "";
                name = $"t{t.TeamId}: {t.TeamName}";
            }

            return new PrintingDocument
            {
                Done = done,
                FileName = p.FileName,
                Id = p.Id,
                Language = p.LanguageId,
                Processed = true,
                Room = room,
                Time = p.Time,
                Team = name,
                SourceCode = Convert.ToBase64String(p.SourceCode),
            };
        }


        /// <summary>
        /// Get the next printing and mark as processed
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <response code="200">The next printing</response>
        [HttpPost]
        public async Task<ActionResult<PrintingDocument>> NextPrinting(int? cid)
        {
            Printing print;

            using (await _locker.LockAsync())
            {
                if (cid.HasValue)
                    print = await _store.FirstAsync(p => p.Done == null && p.ContestId == cid);
                else
                    print = await _store.FirstAsync(p => p.Done == null);

                if (print == null) return new JsonResult("");
                await _store.SetStateAsync(print, false);
            }

            return await SummarizeAsync(print, false);
        }


        /// <summary>
        /// Set the printing as resolved
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="202">The basic information for this printing</response>
        /// <response code="204">The printing has been processed</response>
        [HttpPost("{id}")]
        public async Task<ActionResult<PrintingDocument>> SetDone(
            [FromRoute] int id)
        {
            var p = await _store.FindAsync(id, true);
            if (p == null) return NotFound();
            if (p.Done == true) return NoContent();

            await _store.SetStateAsync(p, true);
            return Accepted(await SummarizeAsync(p, true));
        }
    }
}
