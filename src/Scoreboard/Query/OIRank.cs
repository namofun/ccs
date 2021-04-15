﻿using Ccs.Entities;
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
    /// The queries for scoreboard in OI rules.
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
    /// <c>Score</c> means the last submission score in scoreboard. <br />
    /// <c>IsCorrect</c> means whether the last submission got any score. <br />
    /// <c>SolveTime</c> means the last submission time since contest start in seconds. <br />
    /// <c>FirstToSolve</c> means whether score is full.
    /// </list>
    /// <list type="bullet">
    /// For <see cref="RankCache"/>: <br />
    /// <c>Points</c> means the total of partial scores. <br />
    /// <c>TotalTime</c> means the last submit time.
    /// </list>
    /// </remarks>
    public class OIRank : IRankingStrategy
    {
        /// <inheritdoc />
        public IEnumerable<IScoreboardRow> SortByRule(IEnumerable<IScoreboardRow> source, bool isPublic)
            => isPublic
                ? source.OrderByDescending(a => a.RankCache.PointsPublic)
                    .ThenBy(a => a.RankCache.TotalTimePublic)
                : source.OrderByDescending(a => a.RankCache.PointsRestricted)
                    .ThenBy(a => a.RankCache.TotalTimeRestricted);


        /// <inheritdoc />
        /// <remarks>Accepted is scored as the same way as rejected.</remarks>
        public Task Accept(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args) => Reject(store, contest, args);


        /// <inheritdoc />
        public Task CompileError(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
        {
            bool showPublic = contest.GetState(args.SubmitTime) < ContestState.Frozen;
            return store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
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
                    PendingPublic     = 1,
                    PendingRestricted = 1,
                },

                update: s => new ScoreCache
                {
                    PendingPublic     = s.PendingPublic + 1,
                    PendingRestricted = s.PendingRestricted + 1,
                });
        }


        /// <inheritdoc />
        public async Task Reject(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
        {
            bool showRestricted = contest.GetState(args.SubmitTime) >= ContestState.Frozen;
            double time = (args.SubmitTime - contest.StartTime)?.TotalSeconds ?? 0;
            int time2 = (int)(time / 60);
            int score = args.Judging.TotalScore ?? 0;
            bool fullScore = args.Judging.Status == Verdict.Accepted;
            bool isCorrect = fullScore || score > 0;

            await store.RankUpsertAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,

                insert: s => new RankCache
                {
                    PointsRestricted    = score - (s.ScoreRestricted ?? 0),
                    TotalTimeRestricted = time2,

                    PointsPublic    = showRestricted ? 0 : score - (s.ScoreRestricted ?? 0),
                    TotalTimePublic = showRestricted ? 0 : time2,
                },

                update: (r, s) => new RankCache
                {
                    PointsRestricted    = r.PointsRestricted + s.PointsRestricted,
                    TotalTimeRestricted = s.TotalTimeRestricted,

                    PointsPublic    = r.PointsPublic + s.PointsPublic,
                    TotalTimePublic = showRestricted ? r.TotalTimePublic : time2,
                });

            await store.ScoreUpdateAsync(
                cid: args.ContestId!.Value,
                teamid: args.TeamId,
                probid: args.ProblemId,
                expression: s => new ScoreCache
                {
                    ScoreRestricted      = score,
                    SolveTimeRestricted  = time,
                    IsCorrectRestricted  = isCorrect,
                    PendingRestricted    = s.PendingRestricted - 1,
                    SubmissionRestricted = s.SubmissionRestricted + 1,

                    ScorePublic      = showRestricted ? s.ScorePublic      : score,
                    SolveTimePublic  = showRestricted ? s.SolveTimePublic  : time,
                    IsCorrectPublic  = showRestricted ? s.IsCorrectPublic  : isCorrect,
                    PendingPublic    = showRestricted ? s.PendingPublic    : s.PendingPublic - 1,
                    SubmissionPublic = showRestricted ? s.SubmissionPublic : s.SubmissionPublic + 1,

                    FirstToSolve = fullScore,
                });
        }


        /// <inheritdoc />
        public async Task RefreshCache(IScoreboard store, ScoreboardRefreshEvent args)
        {
            int cid = args.Contest.Id;
            await store.RebuildPartialScoreAsync(cid);
            var results = await store.FetchSolutionsAsync(cid, args.Deadline);

            var rcc = new Dictionary<int, RankCache>();
            var scc = new Dictionary<(int, int), ScoreCache>();
            var lastop1 = new Dictionary<int, int>();
            var lastop2 = new Dictionary<int, int>();

            foreach (var s in results)
            {
                if (!scc.ContainsKey((s.TeamId, s.ProblemId)))
                    scc.Add((s.TeamId, s.ProblemId),
                        new ScoreCache { ContestId = cid, TeamId = s.TeamId, ProblemId = s.ProblemId });
                var sc = scc[(s.TeamId, s.ProblemId)];
                if (s.Status == Verdict.CompileError) continue;

                if (s.Status == Verdict.Running || s.Status == Verdict.Pending)
                {
                    sc.PendingPublic++;
                    sc.PendingRestricted++;
                    continue;
                }

                sc.SubmissionRestricted++;
                sc.IsCorrectRestricted = s.Status == Verdict.Accepted || s.TotalScore != 0;
                sc.ScoreRestricted = s.TotalScore;
                sc.SolveTimeRestricted = (s.Time - args.StartTime).TotalMinutes;
                sc.FirstToSolve = s.Status == Verdict.Accepted;
                if (lastop2.ContainsKey(s.TeamId))
                    lastop2[s.TeamId] = (int)(s.Time - args.StartTime).TotalMinutes;
                else
                    lastop2.Add(s.TeamId, (int)(s.Time - args.StartTime).TotalMinutes);

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
                    if (lastop1.ContainsKey(s.TeamId))
                        lastop1[s.TeamId] = (int)(s.Time - args.StartTime).TotalMinutes;
                    else
                        lastop1.Add(s.TeamId, (int)(s.Time - args.StartTime).TotalMinutes);
                }
            }

            foreach (var r in scc.GroupBy(t => t.Key.Item1, v => v.Value))
            {
                lastop1.TryGetValue(r.Key, out var ttp);
                lastop2.TryGetValue(r.Key, out var ttr);

                var item = new RankCache
                {
                    ContestId = cid,
                    TeamId = r.Key,
                    TotalTimePublic = ttp,
                    TotalTimeRestricted = ttr,
                };

                foreach (var rr in r)
                {
                    item.PointsPublic += rr.ScorePublic ?? 0;
                    item.PointsRestricted += rr.ScoreRestricted ?? 0;
                }

                rcc.Add(r.Key, item);
            }

            await store.RefreshAsync(cid, rcc.Values, scc.Values);
        }
    }
}
