using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class RatingUpdater<TUser, TContext> : RatingUpdaterBase
        where TContext : DbContext
        where TUser : SatelliteSite.IdentityModule.Entities.User, IUserWithRating, new()
    {
        private readonly TContext _context;
        private const int _eligibleCategory = -3; // Category of Participants

        public override int InitialRating => _ratingCalculator.InitialRating;

        public RatingUpdater(TContext context, IRatingCalculator ratingCalculator) : base(ratingCalculator)
        {
            _context = context;
        }

        protected override async Task ApplyRatingChangesAsync(IContestInformation contest, IReadOnlyList<ParticipantRating> results)
        {
            int cid = contest.Id;
            int initialRating = _ratingCalculator.InitialRating;

            // revert the calculated rating delta
            await _context.Set<Member>()
                .Where(m => m.ContestId == cid)
                .BatchUpdateAsync(m => new Member { RatingDelta = null });

            // save the rating delta
            await _context.Set<Member>()
                .BatchUpdateJoinAsync(
                    inner: results.Select(a => new { a.Delta, a.UserId, ContestId = cid }).ToList(),
                    outerKeySelector: m => new { m.ContestId, m.UserId },
                    innerKeySelector: r => new { r.ContestId, r.UserId },
                    updateSelector: (m, r) => new Member { RatingDelta = r.Delta });

            var newRating =
                from r in _context.Set<Member>()
                where r.ContestId == cid && r.RatingDelta != null
                join u in _context.Set<TUser>() on r.UserId equals u.Id
                select new { u.Id, Rating = (u.Rating ?? initialRating) + r.RatingDelta };

            // don't upload data from local
            await _context.Set<TUser>()
                .BatchUpdateJoinAsync(
                    inner: newRating,
                    outerKeySelector: u => u.Id,
                    innerKeySelector: r => r.Id,
                    updateSelector: (u, r) => new TUser { Rating = r.Rating });
        }

        protected override Task<List<ParticipantRating>> GetPreviousRatingsAsync(IContestInformation contest)
        {
            int cid = contest.Id;
            var startTime = contest.StartTime!.Value;
            var endTime = startTime + contest.EndTime!.Value;

            var teamsQuery =
                from t in _context.Set<Team>()
                where t.ContestId == cid && t.CategoryId == _eligibleCategory
                where (from s in _context.Set<Submission>()
                       where s.ContestId == t.ContestId && s.TeamId == t.TeamId
                       where s.Time <= endTime && s.Time >= startTime && !s.Ignored
                       select s).Any()
                join r in _context.Set<RankCache>() on new { t.ContestId, t.TeamId } equals new { r.ContestId, r.TeamId }
                into rcc from r in rcc.DefaultIfEmpty()
                join m in _context.Set<Member>() on new { t.ContestId, t.TeamId } equals new { m.ContestId, m.TeamId }
                select new ParticipantRating(m.UserId, m.PreviousRating, r != null ? r.PointsPublic : 0);

            return teamsQuery.ToListAsync();
        }

        protected override async Task RollbackRatingChangesAsync(IContestInformation contest)
        {
            int cid = contest.Id;
            var concurrencyStamp = Guid.NewGuid().ToString();

            var rollbackQuery =
                from t in _context.Set<Team>()
                where t.ContestId == cid && t.CategoryId == _eligibleCategory
                join m in _context.Set<Member>() on new { t.ContestId, t.TeamId } equals new { m.ContestId, m.TeamId }
                where m.RatingDelta != null
                select new { m.UserId, m.PreviousRating };

            await _context.Set<TUser>()
                .BatchUpdateJoinAsync(
                    inner: rollbackQuery,
                    outerKeySelector: u => u.Id,
                    innerKeySelector: u => u.UserId,
                    updateSelector: (u, u2) => new TUser
                    {
                        Rating = u2.PreviousRating,
                        ConcurrencyStamp = concurrencyStamp,
                    });

            // revert the calculated rating delta
            await _context.Set<Member>()
                .Where(m => m.ContestId == cid)
                .BatchUpdateAsync(m => new Member { RatingDelta = null });
        }

        public override Task<List<RatingListModel>> GetRatedUsersAsync(int page, int count)
        {
            return _context.Set<TUser>()
                .Where(u => u.Rating != null)
                .OrderByDescending(u => u.Rating)
                .Select(u => new RatingListModel(u.Id, u.UserName, (int)u.Rating!))
                .Skip((page - 1) * count)
                .Take(count)
                .ToListAsync();
        }

        public override Task<List<RatingListModel>> GetContestsForUserAsync(int userId)
        {
            return _context.Set<Member>()
                .Where(m => m.UserId == userId)
                .Join(
                    inner: _context.Set<Contest>(),
                    outerKeySelector: m => m.ContestId,
                    innerKeySelector: c => c.Id,
                    resultSelector: (m, c) => new { m, c })
                .OrderBy(a => a.c.StartTime)
                .Select(a => new RatingListModel(a.c.Id, a.c.Name, a.m.RatingDelta))
                .ToListAsync();
        }
    }
}
