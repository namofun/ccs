using Ccs.Entities;
using Ccs.Events;
using Ccs.Models;
using Ccs.Services;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Scoreboard.Query
{
    /// <summary>
    /// The queries for scoreboard in Codeforces rules.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <c>Restricted</c> means all solutions are treated as rejected. <br />
    /// <c>Public</c> means the last accepted solution is treated as accepted. <br />
    /// Several properties are not used and preserved without semantics.
    /// </list>
    /// <list type="bullet">
    /// For <see cref="ScoreCache"/>: <br />
    /// <c>PendingRestricted</c> means the count of pending judgements. <br />
    /// <c>SubmissionRestricted</c> means the total count of non-<see cref="Verdict.CompileError"/> submissions. <br />
    /// <c>SubmissionPublic</c> means the total count of non-<see cref="Verdict.CompileError"/> submissions by the last <b>accepted</b> submission. <br />
    /// <c>ScorePublic</c> means the time related score in scoreboard. <br />
    /// <c>IsCorrectPublic</c> means whether any accepted solutions. <br />
    /// <c>SolveTimePublic</c> means the last accepted submission time since contest start in seconds. <br />
    /// <c>FirstToSolve</c> means whether this solutions fails in system test.
    /// </list>
    /// <list type="bullet">
    /// For <see cref="RankCache"/>: <br />
    /// <c>PointsPublic</c> means the total scores. <br />
    /// <c>TotalTimePublic</c> means the last submit time.
    /// </list>
    /// </remarks>
    public class CFRank : IRankingStrategy
    {
        /// <inheritdoc />
        public IEnumerable<IScoreboardRow> SortByRule(IEnumerable<IScoreboardRow> source, bool isPublic)
            => source.OrderByDescending(a => a.RankCache?.PointsPublic ?? 0)
                .ThenBy(a => a.RankCache?.TotalTimePublic ?? 0);


        /// <inheritdoc />
        public Task CompileError(IScoreboard store, Contest contest, JudgingFinishedEvent args)
        {
            return store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                predicate: s => s.PendingRestricted > 0,
                expression: s => new ScoreCache
                {
                    PendingRestricted = s.PendingRestricted - 1,
                });
        }


        /// <inheritdoc />
        public Task Pending(IScoreboard store, Contest contest, SubmissionCreatedEvent args)
        {
            return store.ScoreUpsertAsync(
                cid: args.Submission.ContestId,
                teamid: args.Submission.TeamId,
                probid: args.Submission.ProblemId,
                expression: s => new ScoreCache
                {
                    PendingRestricted = s.PendingRestricted + 1,
                });
        }


        /// <inheritdoc />
        /// <remarks>
        /// This function will be activated both submitting or rejudging. <br />
        /// If this function is activated by system test, this means the
        /// solution comes from accepted to rejected, and the variable
        /// <c>fst</c> will be true.
        /// </remarks>
        public Task Reject(IScoreboard store, Contest contest, JudgingFinishedEvent args)
        {
            bool fst = args.Judging.Active && args.Judging.RejudgingId.HasValue;

            return store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                expression: s => new ScoreCache
                {
                    SubmissionRestricted = s.SubmissionRestricted + 1,
                    SubmissionPublic = s.IsCorrectPublic ? s.SubmissionPublic : s.SubmissionPublic + 1,
                    FirstToSolve = fst,
                });
        }


        /// <inheritdoc />
        public async Task Accept(IScoreboard store, Contest contest, JudgingFinishedEvent args)
        {
            double time = (args.SubmitTime - contest.StartTime)?.TotalSeconds ?? 0;
            int timee = (int)(time / 60);
            var cfscore = ((JudgingFinishedEvent2)args).CodeforcesScore;
            int minScore = cfscore * 3 / 10, rateScore = cfscore - timee * (cfscore / 250);

            await store.RankUpsertAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                expression: (r, s) => new RankCache
                {
                    PointsPublic = r.PointsPublic - (s.ScorePublic ?? 0)
                        + Math.Max(minScore, rateScore - s.SubmissionRestricted * 50),
                    TotalTimePublic = timee,
                });

            await store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                expression: s => new ScoreCache
                {
                    SubmissionRestricted = s.SubmissionRestricted + 1,
                    PendingRestricted    = s.PendingRestricted - 1,
                    ScorePublic          = Math.Max(minScore, rateScore - s.SubmissionRestricted * 50),
                    IsCorrectPublic      = true,
                    SolveTimePublic      = time,
                    SubmissionPublic     = s.SubmissionRestricted + 1,
                });
        }


        /// <inheritdoc />
        public async Task RefreshCache(IScoreboard store, ScoreboardRefreshEvent args)
        {
            int cid = args.Contest.Id;
            var scores = args.Problems.ToDictionary(k => k.ProblemId, v => v.Score);
            var results = await store.FetchRecalculateAsync(cid, args.Deadline);

            var rcc = new Dictionary<int, RankCache>();
            var scc = new Dictionary<(int, int), ScoreCache>();

            foreach (var s in results)
            {
                if (!scc.ContainsKey((s.TeamId, s.ProblemId)))
                    scc.Add((s.TeamId, s.ProblemId),
                        new ScoreCache { ContestId = cid, TeamId = s.TeamId, ProblemId = s.ProblemId });
                var sc = scc[(s.TeamId, s.ProblemId)];
                if (s.Status == Verdict.CompileError) continue;

                if (s.Status == Verdict.Running || s.Status == Verdict.Pending)
                {
                    sc.PendingRestricted++;
                    continue;
                }

                if (s.Status == Verdict.Accepted)
                {
                    if (!rcc.ContainsKey(s.TeamId))
                        rcc.Add(s.TeamId,
                            new RankCache { ContestId = cid, TeamId = s.TeamId });
                    var rc = rcc[s.TeamId];
                    int score = scores.GetValueOrDefault(sc.ProblemId);

                    var time = sc.SolveTimePublic = (s.Time - args.StartTime).TotalSeconds;
                    var timee = (int)(time / 60);
                    int nowScore = Math.Max(score * 3 / 10, score - timee * (score / 250) - sc.SubmissionRestricted * 50);

                    rc.PointsPublic += nowScore - (sc.ScorePublic ?? 0);
                    rc.TotalTimePublic = timee;

                    sc.ScorePublic = nowScore;
                    sc.SubmissionPublic = sc.SubmissionRestricted + 1;
                    sc.IsCorrectPublic = true;
                    sc.SolveTimePublic = time;
                }

                sc.SubmissionRestricted++;
                sc.SubmissionPublic = sc.IsCorrectPublic ? sc.SubmissionPublic : sc.SubmissionRestricted + 1;
            }

            await store.RefreshAsync(cid, rcc.Values, scc.Values);
        }
    }
}
