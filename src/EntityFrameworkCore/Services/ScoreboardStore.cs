using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ScoreboardStore<TContext> : IScoreboardStore
        where TContext : DbContext
    {
        TContext Context { get; }

        ISubmissionStore Submissions { get; }

        DbSet<ScoreCache> ScoreCache => Context.Set<ScoreCache>();

        DbSet<RankCache> RankCache => Context.Set<RankCache>();

        public ScoreboardStore(TContext context, ISubmissionStore submissions)
        {
            Context = context;
            Submissions = submissions;
        }

        public Task<List<ScoreCalculateModel>> FetchRecalculateAsync(
            int cid, DateTimeOffset deadline)
        {
#warning No SortOrder
            return Submissions.ListWithJudgingAsync(
                predicate: s => s.ContestId == cid && !s.Ignored,
                selector: (s, j) => new ScoreCalculateModel
                {
                    ProblemId = s.ProblemId,
                    Status = j.Status,
                    SubmissionId = s.Id,
                    TeamId = s.TeamId,
                    Time = s.Time,
                    TotalScore = (j.TotalScore ?? 0),
                });
        }

        public Task<Contest> FindContestAsync(int cid)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<int, int>> GetCodeforcesScoreAsync(int cid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsFirstToSolveAsync(
            int cid, int teamid, int probid)
        {
            throw new NotImplementedException();
        }

        public Task RankUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<RankCache, ScoreCache, RankCache>> expression)
        {
            throw new NotImplementedException();
        }

        public Task RebuildPartialScoreAsync(
            int cid)
        {

            throw new NotImplementedException();
        }

        public async Task RefreshAsync(
            int cid,
            IEnumerable<RankCache> ranks,
            IEnumerable<ScoreCache> scores)
        {
            await ScoreCache
                .Where(s => s.ContestId == cid)
                .BatchDeleteAsync();

            await RankCache
                .Where(s => s.ContestId == cid)
                .BatchDeleteAsync();

            ScoreCache.AddRange(scores);
            RankCache.AddRange(ranks);

            await Context.SaveChangesAsync();
        }

        public Task ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, ScoreCache>> expression)
        {
            return ScoreCache
                .Where(sc => sc.ContestId == cid && sc.TeamId == teamid && sc.ProblemId == probid)
                .BatchUpdateAsync(expression);
        }

        public async Task<bool> ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, bool>> predicate,
            Expression<Func<ScoreCache, ScoreCache>> expression)
        {
            return 1 == await ScoreCache
                .Where(sc => sc.ContestId == cid && sc.TeamId == teamid && sc.ProblemId == probid)
                .Where(predicate)
                .BatchUpdateAsync(expression);
        }

        public Task ScoreUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, ScoreCache>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
