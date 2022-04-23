using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Contesting;
using Xylab.Contesting.Models;
using Xylab.Contesting.Services;
using Xylab.Contesting.Specifications;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for scoreboard to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class ScoreboardController : ApiControllerBase<IJuryContext>
    {
        /// <summary>
        /// Get the scoreboard for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="public">Show publicly visible scoreboard, even for users with more permissions</param>
        /// <response code="200">Returns the scoreboard</response>
        [HttpGet]
        public async Task<ActionResult<Scoreboard>> OnGet(
            [FromRoute] int cid,
            [FromQuery] bool @public)
        {
            bool isXcpc = Contest.RankingStrategy == CcsDefaults.RuleXCPC;
            bool isOi = Contest.RankingStrategy == CcsDefaults.RuleIOI;
            if (!Contest.StartTime.HasValue
                || Contest.Feature == CcsDefaults.KindProblemset
                || (!isXcpc && !isOi))
                return null;

            var scb = await Context.GetScoreboardAsync();
            var board = new FullBoardViewModel(scb, @public, !@public);
            var probs = board.Problems;

            var go = board
                .SelectMany(a => a)
                .Select(t => new Scoreboard.Row
                {
                    Rank = t.Rank.Value,
                    TeamId = $"{t.TeamId}",
                    Score = new Scoreboard.Score(isXcpc, t.Points, t.Penalty, t.LastAc),
                    Problems = Enumerable.Range(0, probs.Count).Select(i => MakeProblem(t.Problems[i], probs[i], isXcpc)),
                });

            var maxEvent = await Context.GetMaxEventAsync();
            maxEvent ??= new Xylab.Contesting.Entities.Event { EventTime = DateTimeOffset.Now };
            return new Scoreboard
            {
                Time = AbstractEvent.TrimToMilliseconds(maxEvent.EventTime),
                ContestTime = maxEvent.EventTime - Contest.StartTime.Value,
                EventId = $"{maxEvent.Id}",
                State = new State(Contest),
                Rows = go,
            };
        }

        private Scoreboard.Problem MakeProblem(ScoreCellModel s, ProblemModel p, bool xcpc)
        {
            if (s == null)
            {
                return new Scoreboard.Problem
                {
                    IsPassFail = xcpc,
                    ProblemId = $"{p.ProblemId}",
                    Label = p.ShortName
                };
            }
            else
            {
                return new Scoreboard.Problem
                {
                    IsPassFail = xcpc,
                    FirstToSolve = s.IsFirstToSolve,
                    NumJudged = s.JudgedCount,
                    NumPending = s.PendingCount,
                    ProblemId = $"{p.ProblemId}",
                    Solved = s.Score.HasValue,
                    Score = s.Score ?? 0,
                    Label = p.ShortName,
                    Time = s.Score ?? 0
                };
            }
        }
    }
}
