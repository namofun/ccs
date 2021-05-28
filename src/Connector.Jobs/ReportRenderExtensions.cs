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

            foreach (var g in solt)
            {
                var a2 = g.OrderBy(s => s.Time);
                if (contest.RankingStrategy == 0)
                {
                    var fa = a2.FirstOrDefault(s => s.Verdict == Verdict.Accepted);
                    sids.Add(fa == null ? a2.Last().SubmissionId : fa.SubmissionId);
                }
                else if (contest.RankingStrategy == 1)
                {
                    sids.Add(a2.Last().SubmissionId);
                }
            }

            var sq = await ((ISubmissionContext)context).ListSubmissionsAsync(
                s => new { s.ProblemId, s.SourceCode, s.Id },
                s => sids.Contains(s.Id));

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
            };
        }
    }
}
