﻿using Ccs.Models;
using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for scoreboard to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
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
            if (!Contest.StartTime.HasValue || Contest.Kind == 2)
                return null;
            var scb = await Context.GetScoreboardAsync();
            var affs = await Context.ListAffiliationsAsync();
            var orgs = await Context.ListCategoriesAsync();
            var probs = await Context.ListProblemsAsync();

            var board = new FullBoardViewModel
            {
                RankCache = scb.Data.Values,
                UpdateTime = scb.RefreshTime,
                Problems = probs,
                IsPublic = @public,
                Categories = orgs,
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Affiliations = affs,
            };

            var opt = new int[probs.Count];
            for (int i = 0; i < probs.Count; i++)
                opt[i] = i;

            var go = board
                .SelectMany(a => a)
                .Select(t => new Scoreboard.Row
                {
                    Rank = t.Rank.Value,
                    TeamId = $"{t.TeamId}",
                    Score = new Scoreboard.Score(t.Points, t.Penalty),
                    Problems = opt.Select(i => MakeProblem(t.Problems[i], probs[i]))
                });

            var maxEventId = await Context.GetMaxEventIdAsync();
            return new Scoreboard
            {
                Time = Contest.StartTime.Value,
                ContestTime = DateTimeOffset.Now - Contest.StartTime.Value,
                EventId = $"{maxEventId}",
                State = new State(Contest),
                Rows = go,
            };
        }

        private Scoreboard.Problem MakeProblem(ScoreCellModel s, ProblemModel p)
        {
            if (s == null)
            {
                return new Scoreboard.Problem
                {
                    ProblemId = $"{p.ProblemId}",
                    Label = p.ShortName
                };
            }
            else if (s.Score.HasValue)
            {
                return new Scoreboard.ProblemSolved
                {
                    FirstToSolve = s.IsFirstToSolve,
                    NumJudged = s.JudgedCount,
                    NumPending = s.PendingCount,
                    ProblemId = $"{p.ProblemId}",
                    Solved = s.Score.HasValue,
                    Label = p.ShortName,
                    Time = s.Score ?? 0
                };
            }
            else
            {
                return new Scoreboard.Problem
                {
                    NumJudged = s.JudgedCount,
                    NumPending = s.PendingCount,
                    ProblemId = $"{p.ProblemId}",
                    Solved = s.Score.HasValue,
                    Label = p.ShortName,
                };
            }
        }
    }
}
