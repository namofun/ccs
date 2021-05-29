using Ccs.Connector.Jobs.Models;
using Ccs.Services;
using Polygon.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs
{
    public static class ReportRenderExtensions
    {
        public static async Task<TeamReport> GenerateReport(this IContestContext context, int teamid)
        {
            var team = await context.FindTeamByIdAsync(teamid);
            if (team == null || team.Status != 1) return null;
            var contest = context.Contest;
            if (!contest.StartTime.HasValue || !contest.EndTime.HasValue) return null;
            if (contest.RankingStrategy == 2) return null;

            var start = contest.StartTime.Value;
            var end = start + contest.EndTime.Value;
            var problems = await context.ListProblemsAsync();
            var scoreboard = await context.GetScoreboardAsync();
            var row = scoreboard.Data[team.TeamId];
            var sols = await context.ListSolutionsAsync(teamid: team.TeamId);
            var solt = sols.Where(s => s.Time >= start && s.Time < end).ToLookup(s => s.ProblemId);
            var sids = new List<int>();
            var sid_jids = new Dictionary<int, (int pid, int jid)>();
            var details = new Dictionary<int, IEnumerable<(JudgingRun, Testcase)>>();

            foreach (var prob in problems)
            {
                if (prob.Statement == null)
                {
                    var p2 = await context.FindProblemAsync(prob.ProblemId, true);
                    if (prob != p2) prob.Statement = p2.Statement;
                }
            }

            foreach (var g in solt)
            {
                var a2 = g.OrderBy(s => s.Time);
                Polygon.Models.Solution fa = null;

                if (contest.RankingStrategy == 0)
                {
                    fa = a2.FirstOrDefault(s => s.Verdict == Verdict.Accepted);
                }

                fa ??= a2.Last();
                sids.Add(fa.SubmissionId);
                sid_jids.Add(fa.SubmissionId, (fa.ProblemId, fa.JudgingId));
            }

            var sq = await ((ISubmissionContext)context).ListSubmissionsAsync(
                s => new { s.ProblemId, s.SourceCode, s.Id },
                s => sids.Contains(s.Id));

            foreach (var sc in sid_jids)
            {
                var rt = await ((ISubmissionContext)context).GetDetailsAsync(sc.Value.pid, sc.Value.jid);
                details.Add(sc.Key, rt);
            }

            return new TeamReport
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                ContestId = contest.Id,
                ContestName = contest.Name,
                Rule = contest.RankingStrategy,
                Problems = problems,
                RankCache = row.RankCache,
                ScoreCaches = row.ScoreCache,
                StartTime = start,
                EndTime = end,
                Solutions = solt,
                SourceCodes = sq.ToDictionary(a => a.ProblemId, a => (a.Id, a.SourceCode)),
                Details = details,
            };
        }
    }
}
