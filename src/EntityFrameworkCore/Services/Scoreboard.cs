using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    public class Scoreboard<TContext> : IScoreboard where TContext : DbContext
    {
        public TContext Db { get; }

        public Scoreboard(TContext context)
            => Db = context;

        IQueryable<ScoreCache> ScoreQuery(int cid, int teamid, int probid)
            => from sc in Db.Set<ScoreCache>()
               where sc.ContestId == cid && sc.TeamId == teamid && sc.ProblemId == probid
               select sc;

        IQueryable<ScoreCalculateModel> SolutionQuery(int cid, DateTimeOffset deadline)
            => from s in Db.Set<Submission>()
               where s.ContestId == cid && !s.Ignored && s.Time < deadline
               orderby s.Time ascending
               join j in Db.Set<Judging>() on new { SubmissionId = s.Id, Active = true } equals new { j.SubmissionId, j.Active }
               join t in Db.Set<Team>() on new { s.ContestId, s.TeamId } equals new { t.ContestId, t.TeamId }
               join c in Db.Set<Category>() on t.CategoryId equals c.Id
               select new ScoreCalculateModel
               {
                   ProblemId = s.ProblemId,
                   Status = j.Status,
                   SubmissionId = s.Id,
                   TeamId = s.TeamId,
                   Time = s.Time,
                   TotalScore = j.TotalScore ?? 0,
                   SortOrder = c.SortOrder,
               };

        IQueryable<int> SortOrderQuery(int cid, int teamid)
            => from t in Db.Set<Team>()
               where t.ContestId == cid && t.TeamId == teamid
               join c in Db.Set<Category>() on t.CategoryId equals c.Id
               select c.SortOrder;

        IQueryable<ScoreCache> FirstBloodQuery(int cid, int probid, IQueryable<int> sortOrders)
            => from sc in Db.Set<ScoreCache>()
               where sc.ContestId == cid && sc.ProblemId == probid && sc.FirstToSolve
               join t in Db.Set<Team>() on new { sc.ContestId, sc.TeamId } equals new { t.ContestId, t.TeamId }
               join c in Db.Set<Category>() on t.CategoryId equals c.Id
               where sortOrders.Contains(c.SortOrder)
               select sc;

        IQueryable<PartialScore> RealScoreQuery(int cid)
            => from s in Db.Set<Submission>()
               where s.ContestId == cid && !s.Ignored
               join j in Db.Set<Judging>() on s.Id equals j.SubmissionId
               where j.Status < Verdict.Pending || j.Status > Verdict.Running
               join jr in Db.Set<JudgingRun>() on j.Id equals jr.JudgingId
               join t in Db.Set<Testcase>() on jr.TestcaseId equals t.Id
               group new { jr.Status, t.Point } by j.Id into g
               select new PartialScore { Id = g.Key, Score = g.Sum(a => a.Status == Verdict.Accepted ? a.Point : 0) };

        IQueryable<SubmissionStatistics> Statistics(int cid)
            => from s in Db.Set<Submission>()
               where s.ContestId == cid
               join j in Db.Set<Judging>() on new { s.Id, Active = true } equals new { Id = j.SubmissionId, j.Active }
               group j.Status by new { s.ProblemId, s.TeamId, s.ContestId } into g
               select new SubmissionStatistics
               {
                   ProblemId = g.Key.ProblemId,
                   TeamId = g.Key.TeamId,
                   ContestId = g.Key.ContestId,
                   TotalSubmission = g.Count(),
                   AcceptedSubmission = g.Sum(v => v == Verdict.Accepted ? 1 : 0)
               };

        public Task<List<ScoreCalculateModel>> FetchSolutionsAsync(int cid, DateTimeOffset deadline)
            => SolutionQuery(cid, deadline).ToListAsync();

        public Task<bool> IsFirstToSolveAsync(int cid, int teamid, int probid)
            => FirstBloodQuery(cid, probid, SortOrderQuery(cid, teamid)).AnyAsync();

        public Task RebuildPartialScoreAsync(int cid)
            => Db.Set<Judging>().BatchUpdateJoinAsync(
                RealScoreQuery(cid), j => j.Id, p => p.Id,
                (j, p) => new Judging { TotalScore = p.Score });

        public Task ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, ScoreCache>> expression)
            => ScoreQuery(cid, teamid, probid).BatchUpdateAsync(expression);

        public async Task<bool> ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, bool>> predicate,
            Expression<Func<ScoreCache, ScoreCache>> expression)
            => 1 == await ScoreQuery(cid, teamid, probid)
                .Where(predicate).BatchUpdateAsync(expression);

        public Task RankUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, RankCache>> insert,
            Expression<Func<RankCache, RankCache, RankCache>> update)
            => RankUpsertInnerAsync(ScoreQuery(cid, teamid, probid), insert, update);

        public Task ScoreUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache>> insert,
            Expression<Func<ScoreCache, ScoreCache>> update)
            => ScoreUpsertInnerAsync(
                new { cid, teamid, probid }, insert, update,
                a => new ScoreCache { ContestId = a.cid, TeamId = a.teamid, ProblemId = a.probid, });

        public async Task RefreshAsync(int cid, IEnumerable<RankCache> ranks, IEnumerable<ScoreCache> scores)
        {
            await Db.Set<ScoreCache>().Where(s => s.ContestId == cid).BatchDeleteAsync();
            await Db.Set<RankCache>().Where(s => s.ContestId == cid).BatchDeleteAsync();

            Db.Set<ScoreCache>().AddRange(scores);
            Db.Set<RankCache>().AddRange(ranks);
            await Db.SaveChangesAsync();
        }

        Task RankUpsertInnerAsync(
            IQueryable<ScoreCache> source,
            Expression<Func<ScoreCache, RankCache>> insert,
            Expression<Func<RankCache, RankCache, RankCache>> update)
        {
            var insertBody = (MemberInitExpression)insert.Body;
            var param = insert.Parameters[0];

            insert = Expression.Lambda<Func<ScoreCache, RankCache>>(
                Expression.MemberInit(
                    insertBody.NewExpression,
                    insertBody.Bindings
                        .Append(Expression.Bind(RankCache_ContestId, Expression.Property(param, ScoreCache_ContestId)))
                        .Append(Expression.Bind(RankCache_TeamId, Expression.Property(param, ScoreCache_TeamId)))),
                param);

            return Db.Set<RankCache>().UpsertAsync(source, insert, update);
        }

        Task ScoreUpsertInnerAsync<T>(
            T entry,
            Expression<Func<ScoreCache>> insert,
            Expression<Func<ScoreCache, ScoreCache>> update,
            Expression<Func<T, ScoreCache>> primaryKey)
            where T : class
        {
            var insertBody = (MemberInitExpression)insert.Body;
            var pkeyBody = (MemberInitExpression)primaryKey.Body;

            var ins2 = Expression.Lambda<Func<T, ScoreCache>>(
                Expression.MemberInit(
                    insertBody.NewExpression,
                    Enumerable.Concat(insertBody.Bindings, pkeyBody.Bindings)),
                primaryKey.Parameters);

            var upd2 = Expression.Lambda<Func<ScoreCache, ScoreCache, ScoreCache>>(
                update.Body,
                update.Parameters[0],
                Expression.Parameter(typeof(ScoreCache), "__"));

            return Db.Set<ScoreCache>().UpsertAsync(entry, ins2, upd2);
        }

        public Task CreateBalloonAsync(int id)
        {
            Db.Set<Balloon>().Add(new Balloon { SubmissionId = id });
            return Db.SaveChangesAsync();
        }

        public Task EmitEventAsync(Event @event)
        {
            Db.Set<Event>().Add(@event);
            return Db.SaveChangesAsync();
        }

        public async Task RebuildStatisticsAsync(int cid)
        {
            await Db.Set<SubmissionStatistics>()
                .Where(s => s.ContestId == cid)
                .BatchDeleteAsync();

            await Db.Set<SubmissionStatistics>().UpsertAsync(

                sources: Statistics(cid),

                updateExpression: (_, ss) => new SubmissionStatistics
                {
                    AcceptedSubmission = ss.AcceptedSubmission,
                    TotalSubmission = ss.TotalSubmission
                },

                insertExpression: ss => new SubmissionStatistics
                {
                    TeamId = ss.TeamId,
                    ContestId = ss.ContestId,
                    ProblemId = ss.ProblemId,
                    AcceptedSubmission = ss.AcceptedSubmission,
                    TotalSubmission = ss.TotalSubmission
                });
        }

        public async Task<IReadOnlyDictionary<int, int>> GetModeScoresAsync(int cid)
            => await Db.Set<ContestProblem>()
                .Where(cp => cp.ContestId == cid)
                .Select(cp => new { cp.ProblemId, cp.Score })
                .ToDictionaryAsync(a => a.ProblemId, a => a.Score);

        private static readonly PropertyInfo RankCache_ContestId = typeof(RankCache).GetProperty(nameof(Team.ContestId))!;
        private static readonly PropertyInfo RankCache_TeamId = typeof(RankCache).GetProperty(nameof(Team.TeamId))!;
        private static readonly PropertyInfo ScoreCache_ContestId = typeof(ScoreCache).GetProperty(nameof(Team.ContestId))!;
        private static readonly PropertyInfo ScoreCache_TeamId = typeof(ScoreCache).GetProperty(nameof(Team.TeamId))!;
        private static readonly PropertyInfo ScoreCache_ProblemId = typeof(ScoreCache).GetProperty(nameof(ContestProblem.ProblemId))!;
    }
}
