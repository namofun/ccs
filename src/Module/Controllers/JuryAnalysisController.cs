﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Contesting.Models;
using Xylab.Contesting.Services;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(3)}/jury/analysis")]
    public class JuryAnalysisController : JuryControllerBase<IAnalysisContext>
    {
        public IReadOnlyDictionary<int, AnalyticalTeam> Teams { get; set; }

        private static void Add<T>(Dictionary<T, int> kvp, T key)
        {
            kvp[key] = kvp.GetValueOrDefault(key) + 1;
        }

        private static void MaxEqual<T>(Dictionary<T, int> kvp, T key, int value)
        {
            kvp[key] = Math.Max(kvp.GetValueOrDefault(key), value);
        }

        private static string Colors(Verdict verdict)
        {
            return verdict switch
            {
                Verdict.Accepted => "#01df01",
                Verdict.WrongAnswer => "red",
                Verdict.TimeLimitExceeded => "orange",
                Verdict.RuntimeError => "#ff3399",
                Verdict.MemoryLimitExceeded => "purple",
                Verdict.CompileError => "grey",
                Verdict.OutputLimitExceeded => "black",
                _ => "grey",
            };
        }

        public override async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            await base.OnActionExecutingAsync(context);
            if (context.Result == null && !Contest.StartTime.HasValue)
            {
                context.Result = StatusCodePage(StatusCodes.Status410Gone);
                return;
            }

            if (!Contest.EndTime.HasValue || Contest.EndTime.Value.TotalMinutes >= 1440)
            {
                context.Result = StatusCodePage(StatusCodes.Status503ServiceUnavailable);
                return;
            }

            Teams = await Context.GetAnalyticalTeamsAsync();
        }


        [HttpGet]
        public async Task<IActionResult> Overview()
        {
            int cid = Contest.Id;
            var startTime = Contest.StartTime.Value;
            var endTime = startTime + Contest.EndTime.Value;

            var result = await Context.ListSolutionsAsync(
                predicate: s => s.Time >= startTime && s.Time <= endTime,
                selector: (s, j) => new { s.Time, j.Status, s.ProblemId, s.TeamId, s.Language });

            var tof = (int)Math.Ceiling(Contest.EndTime.Value.TotalMinutes);
            var model = new AnalysisOneModel(tof);
            var dbl = model.VerdictStatistics;
            int toc = 0;

            foreach (var stat in result)
            {
                if (!Teams.ContainsKey(stat.TeamId)) continue;
                toc++;
                Add(model.AttemptedLanguages, stat.Language);
                Add(model.AttemptedProblems, stat.ProblemId);
                int thisTime = (int)Math.Ceiling((stat.Time - startTime).TotalMinutes);

                dbl[(int)stat.Status, thisTime]++;
                var keyid = (stat.TeamId, stat.ProblemId);
                var valv = model.TeamStatistics.GetValueOrDefault(keyid);
                valv = (valv.ac, valv.at + 1);

                if (stat.Status == Verdict.Accepted)
                {
                    Add(model.AcceptedLanguages, stat.Language);
                    Add(model.AcceptedProblems, stat.ProblemId);
                    MaxEqual(model.TeamLastSubmission, stat.TeamId, thisTime);
                    valv = (valv.ac + 1, valv.at);
                }

                model.TeamStatistics[keyid] = valv;
            }

            foreach (var langid in model.AcceptedLanguages.Keys.Union(model.AcceptedLanguages.Keys).ToHashSet())
            {
                model.AcceptedLanguages[langid] = model.AcceptedLanguages.GetValueOrDefault(langid);
                model.AttemptedLanguages[langid] = model.AttemptedLanguages.GetValueOrDefault(langid);
            }

            for (int i = 0; i < 12; i++)
                for (int j = 1; j <= tof; j++)
                    dbl[i, j] += dbl[i, j - 1];

            model.TotalSubmissions = toc;
            model.Teams = Teams;
            model.Problems = await Context.ListProblemsAsync();
            return View(model);
        }


        [HttpGet("[action]/{probid}")]
        public async Task<IActionResult> Problem(int probid)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            int cid = Contest.Id;
            var startTime = Contest.StartTime.Value;
            var endTime = startTime + Contest.EndTime.Value;

            var result = await Context.ListSolutionsAsync(
                predicate: s => s.Time >= startTime && s.Time <= endTime && s.ProblemId == probid,
                selector: (s, j) => new { s.Time, SubmissionId = s.Id, j.Status, s.TeamId, s.Language, JudgingId = j.Id, j.ExecuteTime });

            var tof = (int)Math.Ceiling((endTime - startTime).TotalMinutes);
            var model = new AnalysisTwoModel(tof, prob);
            var dbl = model.VerdictStatistics;
            int toc = 0, toac = 0;
            var set1 = new HashSet<int>();
            var set2 = new HashSet<int>();

            foreach (var stat in result)
            {
                if (!Teams.ContainsKey(stat.TeamId)) continue;
                toc++;
                int thisTime = (int)Math.Ceiling((stat.Time - startTime).TotalMinutes);
                set1.Add(stat.TeamId);
                dbl[(int)stat.Status, thisTime]++;

                if (stat.Status == Verdict.Accepted)
                {
                    toac++;
                    set2.Add(stat.TeamId);
                }
            }

            for (int i = 0; i < 12; i++)
                for (int j = 1; j <= tof; j++)
                    dbl[i, j] += dbl[i, j - 1];

            model.TotalSubmissions = toc;
            model.TotalAccepted = toac;
            model.TeamAccepted = set2.Count;
            model.TeamAttempted = set1.Count;

            model.List = result
                .Where(a => a.ExecuteTime.HasValue && Teams.ContainsKey(a.TeamId))
                .OrderBy(a => a.ExecuteTime.Value)
                .Select(a => new
                {
                    id = a.SubmissionId,
                    label = $"j{a.JudgingId}",
                    value = a.ExecuteTime / 1000.0,
                    team = Teams[a.TeamId].TeamName ?? "undefined",
                    submittime = a.Time - startTime,
                    color = Colors(a.Status),
                });

            return View(model);
        }
    }
}
