using System.Threading.Tasks;

namespace Ccs.Scoreboard.Ratings
{
    public class RatingUpdater
    {
        //DbContext Context { get; }

        //public RatingUpdater(DbContextAccessor context)
        //{
        //    Context = context;
        //}

        public async Task RollbackAsync(int cid)
        {
            /*
            var rollbackQuery =
                from t in Context.Set<Team>()
                where t.ContestId == cid && t.Eligible
                join m in Context.Set<TeamMember>() on new { t.ContestId, t.TeamId } equals new { m.ContestId, m.TeamId }
                where m.RatingDelta != null
                select new { m.UserId, m.PreviousRating };

            await Context.Set<User>().MergeAsync(
                sourceTable: rollbackQuery,
                targetKey: u => u.Id,
                sourceKey: u => u.UserId,
                insertExpression: null,
                delete: false,
                updateExpression: (u, u2) => new User { Rating = u2.PreviousRating });
            */
        }

        public async Task UpdateAsync(int cid)
        {
            /*
            var contest = await Context.Set<Contest>().FindAsync(cid);
            if (contest == null) throw new ApplicationException();

            var teamsQuery =
                from t in Context.Set<Team>()
                where t.ContestId == cid && t.Eligible
                join s in Context.Set<Submission>() on new { t.ContestId, Author = t.TeamId } equals new { s.ContestId, s.Author }
                where s.Time < contest.EndTime && s.Time > contest.StartTime && !s.Ignored
                group 1 by new { t.ContestId, t.TeamId } into g
                select new { g.Key.ContestId, g.Key.TeamId, Count = g.Count() } into t
                join r in Context.Set<RankCache>() on new { t.ContestId, t.TeamId } equals new { r.ContestId, r.TeamId }
                into rcc from r in rcc.DefaultIfEmpty()
                join m in Context.Set<TeamMember>() on new { t.ContestId, t.TeamId } equals new { m.ContestId, m.TeamId }
                select new Participant
                {
                    UserId = m.UserId,
                    Points = r != null ? r.PointsPublic : 0,
                    UserRating = m.PreviousRating
                };

            var participants = await teamsQuery.ToListAsync();
            participants.ProcessComputing();

            await Context.Set<TeamMember>()
                .Where(m => m.ContestId == cid)
                .BatchUpdateAsync(m => new TeamMember { RatingDelta = null });

            // 保存RatingChange曲线
            var rc = participants.Select(a => new { a.Delta, a.UserId, cid });
            await Context.Set<TeamMember>()
                .MergeAsync(
                    sourceTable: rc,
                    targetKey: m => new { m.ContestId, m.UserId },
                    sourceKey: r => new { ContestId = r.cid, r.UserId },
                    updateExpression: (m, r) => new TeamMember { RatingDelta = r.Delta },
                    insertExpression: null, delete: false);

            var newRating =
                from r in Context.Set<TeamMember>()
                where r.ContestId == cid && r.RatingDelta != null
                join u in Context.Set<User>() on r.UserId equals u.Id
                select new { u.Id, Rating = (u.Rating ?? RatingCalculator.INITIAL_RATING) + r.RatingDelta };

            // 批量更新
            await Context.Set<User>()
                .MergeAsync(
                    sourceTable: newRating,
                    targetKey: u => u.Id,
                    sourceKey: r => r.Id,
                    updateExpression: (u, r) => new User { Rating = r.Rating },
                    insertExpression: null, delete: false);
            */
        }
    }
}
