using Ccs.Entities;
using Ccs.Events;
using Ccs.Models;
using Ccs.Services;
using Polygon.Entities;
using Polygon.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Scoreboard.Query
{
    /// <summary>
    /// The queries for scoreboard in XCPC rules.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <c>Restricted</c> means the inner board. <br />
    /// <c>Public</c> means the public board.
    /// </list>
    /// <list type="bullet">
    /// For <see cref="ScoreCache"/>: <br />
    /// <c>Pending</c> means the count of pending judgements. <br />
    /// <c>Submission</c> means the count of non-<see cref="Verdict.CompileError"/> submissions. <br />
    /// <c>Score</c> means the solving time in scoreboard. <br />
    /// <c>IsCorrect</c> means whether the last submission is judged as <see cref="Verdict.Accepted"/>. <br />
    /// <c>SolveTime</c> means the submission time since contest start in seconds. <br />
    /// <c>FirstToSolve</c> means whether this submission is first to solve in this sort order.
    /// </list>
    /// <list type="bullet">
    /// For <see cref="RankCache"/>: <br />
    /// <c>Points</c> means the total count of accepted problems. <br />
    /// <c>TotalTime</c> means the penalty time.
    /// </list>
    /// </remarks>
    public class XCPCRank : IRankingStrategy
    {
        /// <inheritdoc />
        public IEnumerable<IScoreboardRow> SortByRule(IEnumerable<IScoreboardRow> source, bool isPublic)
            => isPublic
                ? source.OrderByDescending(a => a.RankCache.PointsPublic)
                    .ThenBy(a => a.RankCache.TotalTimePublic)
                    .ThenBy(a => a.RankCache.LastAcPublic)
                    .ThenBy(a => a.TeamName)
                : source.OrderByDescending(a => a.RankCache.PointsRestricted)
                    .ThenBy(a => a.RankCache.TotalTimeRestricted)
                    .ThenBy(a => a.RankCache.LastAcRestricted)
                    .ThenBy(a => a.TeamName);


        /// <inheritdoc />
        /// <remarks>The race condition caused by FirstToSolve query can be ignored. FirstToSolve field isn't important.</remarks>
        public async Task Accept(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
        {
            bool fb = await store.IsFirstToSolveAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId);

            double time = (args.SubmitTime - contest.StartTime)?.TotalSeconds ?? 0;
            int score = time < 0 ? -(((int)-time) / 60) : ((int)time) / 60;
            bool showRestricted = contest.GetState(args.SubmitTime) >= ContestState.Frozen;

            bool hit = await store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                predicate: s => s.PendingRestricted > 0,
                expression: s => new ScoreCache
                {
                    PendingPublic    = showRestricted ? s.PendingPublic    : s.PendingPublic - 1,
                    SubmissionPublic = showRestricted ? s.SubmissionPublic : s.SubmissionPublic + 1,
                    ScorePublic      = showRestricted ? (int?)null         : score,
                    SolveTimePublic  = showRestricted ? (double?)null      : time,
                    IsCorrectPublic  = showRestricted,

                    PendingRestricted    = s.PendingRestricted - 1,
                    SubmissionRestricted = s.SubmissionRestricted + 1,
                    ScoreRestricted      = score,
                    SolveTimeRestricted  = time,
                    IsCorrectRestricted  = true,

                    FirstToSolve = fb,
                });

            if (!hit) return;

            await store.RankUpsertAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,

                insert: s => new RankCache
                {
                    PointsRestricted    = 1,
                    TotalTimeRestricted = score + 20 * (s.SubmissionRestricted - 1),
                    LastAcRestricted    = score,

                    PointsPublic    = showRestricted ? 0 : 1,
                    TotalTimePublic = showRestricted ? 0 : score + 20 * (s.SubmissionRestricted - 1),
                    LastAcPublic    = showRestricted ? 0 : score,
                },

                update: (r, e) => new RankCache
                {
                    PointsRestricted    = r.PointsRestricted    + e.PointsRestricted,
                    TotalTimeRestricted = r.TotalTimeRestricted + e.TotalTimeRestricted,
                    LastAcRestricted    = r.LastAcRestricted > e.LastAcRestricted ? r.LastAcRestricted : e.LastAcRestricted,

                    PointsPublic    = r.PointsPublic    + e.PointsPublic,
                    TotalTimePublic = r.TotalTimePublic + e.TotalTimePublic,
                    LastAcPublic    = showRestricted || r.LastAcPublic > e.LastAcPublic ? r.LastAcPublic : e.LastAcPublic,
                });

            if (showRestricted || !contest.Settings.BalloonAvailable) return;
            await store.CreateBalloonAsync(args.Judging.SubmissionId);
        }


        /// <inheritdoc />
        public Task CompileError(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
        {
            bool showPublic = contest.GetState(args.SubmitTime) < ContestState.Frozen;
            return store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                predicate: s => s.PendingRestricted > 0,
                expression: s => new ScoreCache
                {
                    PendingPublic = showPublic ? s.PendingPublic - 1 : s.PendingPublic,
                    PendingRestricted = s.PendingRestricted - 1,
                });
        }


        /// <inheritdoc />
        public Task Pending(IScoreboard store, IContestInformation contest, SubmissionCreatedEvent args)
        {
            return store.ScoreUpsertAsync(
                cid: args.Submission.ContestId,
                teamid: args.Submission.TeamId,
                probid: args.Submission.ProblemId,

                insert: () => new ScoreCache
                {
                    PendingPublic = 1,
                    PendingRestricted = 1,
                },

                update: s => new ScoreCache
                {
                    PendingPublic = s.IsCorrectRestricted ? s.PendingPublic : s.PendingPublic + 1,
                    PendingRestricted = s.IsCorrectRestricted ? s.PendingRestricted : s.PendingRestricted + 1,
                });
        }


        /// <inheritdoc />
        public Task Reject(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
        {
            bool showPublic = contest.GetState(args.SubmitTime) < ContestState.Frozen;
            return store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                predicate: s => s.PendingRestricted > 0,
                expression: s => new ScoreCache
                {
                    PendingPublic = showPublic ? s.PendingPublic - 1 : s.PendingPublic,
                    SubmissionPublic = showPublic ? s.SubmissionPublic + 1 : s.SubmissionPublic,
                    PendingRestricted = s.PendingRestricted - 1,
                    SubmissionRestricted = s.SubmissionRestricted + 1,
                });
        }


        /// <inheritdoc />
        public async Task<ScoreboardRawData> RefreshCache(IScoreboard store, ScoreboardRefreshEvent args)
        {
            int cid = args.Contest.Id;
            var results = await store.FetchSolutionsAsync(cid, args.Deadline);
            var rcc = new Dictionary<int, RankCache>();
            var scc = new Dictionary<(int, int), ScoreCache>();
            var fb = new HashSet<(int, int)>();
            var oks = new List<int>();
            var balloons = new List<int>();

            foreach (var s in results)
            {
                if (!scc.ContainsKey((s.TeamId, s.ProblemId)))
                    scc.Add((s.TeamId, s.ProblemId),
                        new ScoreCache { ContestId = cid, TeamId = s.TeamId, ProblemId = s.ProblemId });
                var sc = scc[(s.TeamId, s.ProblemId)];
                if (sc.IsCorrectRestricted || s.Status == Verdict.CompileError) continue;

                if (s.Status == Verdict.Running || s.Status == Verdict.Pending)
                {
                    sc.PendingPublic++;
                    sc.PendingRestricted++;
                    continue;
                }

                sc.SubmissionRestricted++;

                if (s.Status == Verdict.Accepted)
                {
                    if (!rcc.ContainsKey(s.TeamId))
                        rcc.Add(s.TeamId,
                            new RankCache { ContestId = cid, TeamId = s.TeamId });
                    var rc = rcc[s.TeamId];
                    var timee = (s.Time - args.StartTime).TotalSeconds;

                    sc.IsCorrectRestricted = true;
                    sc.SolveTimeRestricted = timee;
                    sc.ScoreRestricted = timee < 0 ? -(((int)-timee) / 60) : ((int)timee) / 60;
                    oks.Add(s.SubmissionId);

                    int penalty = (sc.SubmissionRestricted - 1) * 20 + sc.ScoreRestricted.Value;
                    rc.PointsRestricted++;
                    rc.TotalTimeRestricted += penalty;
                    rc.LastAcRestricted = sc.ScoreRestricted.Value;

                    if (!fb.Contains((s.ProblemId, s.SortOrder)))
                    {
                        fb.Add((s.ProblemId, s.SortOrder));
                        sc.FirstToSolve = true;
                    }
                }

                if (args.FreezeTime.HasValue && s.Time >= args.FreezeTime.Value)
                {
                    sc.PendingPublic++;
                }
                else
                {
                    sc.SolveTimePublic = sc.SolveTimeRestricted;
                    sc.SubmissionPublic = sc.SubmissionRestricted;
                    sc.IsCorrectPublic = sc.IsCorrectRestricted;
                    sc.ScorePublic = sc.ScoreRestricted;

                    if (s.Status == Verdict.Accepted)
                    {
                        var rc = rcc[s.TeamId];
                        rc.PointsPublic = rc.PointsRestricted;
                        rc.TotalTimePublic = rc.TotalTimeRestricted;
                        rc.LastAcPublic = rc.LastAcRestricted;
                        balloons.Add(s.SubmissionId);
                    }
                }
            }

            if (!args.Contest.Settings.BalloonAvailable) balloons.Clear();
            return new ScoreboardRawData(cid, rcc.Values, scc.Values, balloons);
        }
    }
}
