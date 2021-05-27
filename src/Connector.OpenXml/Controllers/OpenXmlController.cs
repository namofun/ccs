using Ccs.Models;
using Ccs.Scoreboard;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.OpenXml.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    public class OpenXmlController : JuryControllerBase<IContestContext>
    {
        [HttpGet("[area]/{cid:c(3)}/jury/import-export/[action]")]
        public async Task<IActionResult> ScoreboardXlsx(
            [FromQuery] int[] affiliations = null,
            [FromQuery] int[] categories = null)
        {
            if (Contest.RankingStrategy == CcsDefaults.RuleCodeforces) return StatusCode(501);
            if (affiliations != null && affiliations.Length == 0) affiliations = null;
            if (categories != null && categories.Length == 0) categories = null;

            if (!Contest.StartTime.HasValue) return BadRequest();
            var scb = await Context.GetScoreboardAsync();
            var affs = await Context.ListAffiliationsAsync();
            var orgs = await Context.ListCategoriesAsync();
            var probs = await Context.ListProblemsAsync();

            var board = new FullBoardViewModel
            {
                UpdateTime = scb.RefreshTime,
                Problems = probs,
                IsPublic = false,
                Categories = orgs,
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Affiliations = affs,
                RankCache = scb.Data.Values
                    .WhereIf(affiliations != null, r => affiliations.Contains(r.AffiliationId))
                    .WhereIf(categories != null, r => categories.Contains(r.CategoryId)),
            };

            using var workbook = OpenXmlScoreboard.Create(board, Contest.Name);

            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return File(
                fileStream: memoryStream,
                contentType: OpenXmlScoreboard.MimeType,
                fileDownloadName: $"c{Contest.Id}-scoreboard.xlsx");
        }
    }
}
