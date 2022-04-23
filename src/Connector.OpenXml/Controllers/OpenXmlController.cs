using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Controllers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Contesting.Models;
using Xylab.Contesting.Scoreboard;
using Xylab.Contesting.Services;

namespace Xylab.Contesting.Connector.OpenXml.Controllers
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
            var board = new FullBoardViewModel(scb, false)
            {
                FilteredAffiliations = affiliations?.ToHashSet(),
                FilteredCategories = categories?.ToHashSet(),
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
