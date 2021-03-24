using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ImmediateContestRepository2<TContext> : IContestRepository2
        where TContext : DbContext, IContestDbContext
    {
        public TContext Db { get; }

        public ImmediateContestRepository2(TContext context)
            => Db = context;

        public Task<IPagedList<ContestListModel>> ListAsync(
            ClaimsPrincipal user,
            int? kind = null,
            int page = 1,
            int limit = 100)
        {
            IQueryable<ContestListModel> baseQuery;

            if (!int.TryParse(user.GetUserId(), out int UserId))
            {
                baseQuery = Db.Contests
                    .Where(c => c.IsPublic)
                    .WhereIf(kind.HasValue, c => c.Kind == kind)
                    .OrderByDescending(c => c.Id)
                    .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount));
            }
            else if (int.TryParse(user.FindFirst("tenant")?.Value, out int AffiliationId))
            {
                baseQuery = Db.Contests
                    .LeftJoin(
                        inner: Db.TeamMembers,
                        outerKeySelector: c => new { ContestId = c.Id, UserId },
                        innerKeySelector: m => new { m.ContestId, m.UserId },
                        resultSelector: (c, m) => new { c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, TeamId = (int?)m.TeamId })
                    .LeftJoin(
                        inner: Db.ContestTenants,
                        outerKeySelector: c => new { ContestId = c.Id, AffiliationId },
                        innerKeySelector: v => new { v.ContestId, v.AffiliationId },
                        resultSelector: (c, v) => new { c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, c.TeamId, AffiliationId = (int?)v.AffiliationId })
                    .Where(c => c.IsPublic || c.TeamId != null || c.AffiliationId != null)
                    .WhereIf(kind.HasValue, c => c.Kind == kind)
                    .OrderByDescending(c => c.Id)
                    .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, c.TeamId != null));
            }
            else
            {
                baseQuery = Db.Contests
                    .LeftJoin(
                        inner: Db.TeamMembers,
                        outerKeySelector: c => new { ContestId = c.Id, UserId },
                        innerKeySelector: m => new { m.ContestId, m.UserId },
                        resultSelector: (c, m) => new { c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, TeamId = (int?)m.TeamId })
                    .Where(c => c.IsPublic || c.TeamId != null)
                    .WhereIf(kind.HasValue, c => c.Kind == kind)
                    .OrderByDescending(c => c.Id)
                    .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, c.TeamId != null));
            }

            return baseQuery.ToPagedListAsync(page, limit);
        }

        public Task<List<ProblemsetStatistics>> StatisticsAsync(int contestId, int teamId)
        {
            return Db.SubmissionStatistics
                .Where(c => c.ContestId == contestId && c.TeamId == teamId)
                .Join(
                    inner: Db.ContestProblems,
                    outerKeySelector: s => new { s.ContestId, s.ProblemId },
                    innerKeySelector: p => new { p.ContestId, p.ProblemId },
                    resultSelector: (s, p) => new ProblemsetStatistics
                    {
                        AcceptedSubmission = s.AcceptedSubmission,
                        ProblemId = p.ShortName,
                        TotalSubmission = s.TotalSubmission,
                    })
                .ToListAsync();
        }
    }

    public sealed class CachedContestRepository2Cache : IMemoryCache
    {
        private readonly IMemoryCache _memoryCache;

        public ICacheEntry CreateEntry(object key)
        {
            return _memoryCache.CreateEntry(key);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        public void Remove(object key)
        {
            _memoryCache.Remove(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public CachedContestRepository2Cache()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                Clock = new Microsoft.Extensions.Internal.SystemClock(),
            });
        }
    }

    public class CachedContestRepository2<TContext> : IContestRepository2
        where TContext : DbContext, IContestDbContext
    {
        private readonly HashSet<int> _emptySet = new HashSet<int>();

        public TContext Db { get; }

        public IMemoryCache Cache { get; }

        public Task<List<Contest>> GetContests()
            => CachedGetAsync("Contests", 5,
                () => Db.Contests.AsNoTracking()
                    .OrderByDescending(c => c.Id)
                    .ToListAsync());

        public Task<HashSet<int>> GetVisibility(int affId)
            => CachedGetAsync($"Tenant({affId})", 5,
                () => Db.ContestTenants
                    .Where(a => a.AffiliationId == affId)
                    .Select(a => a.ContestId)
                    .ToHashSetAsync());

        public Task<HashSet<int>> GetRegistered(int userId)
            => Queryable
                .Concat(
                    Db.TeamMembers
                        .Where(m => m.UserId == userId)
                        .Select(m => m.ContestId),
                    Db.ContestJuries
                        .Where(j => j.UserId == userId)
                        .Select(j => j.ContestId))
                .ToHashSetAsync();

        public CachedContestRepository2(
            TContext context,
            CachedContestRepository2Cache cache)
        {
            Db = context;
            Cache = cache;
        }

        public Task<T> CachedGetAsync<T>(string key, int time, Func<Task<T>> valueFactory)
        {
            return Cache.GetOrCreateAsync(key, async entry =>
            {
                var result = await valueFactory();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(time);
                return result;
            });
        }

        public async Task<IPagedList<ContestListModel>> ListAsync(
            ClaimsPrincipal user,
            int? kind = null,
            int page = 1,
            int limit = 100)
        {
            var contests = await GetContests();
            var users = _emptySet;
            var affs = _emptySet;

            if (int.TryParse(user.GetUserId(), out int userId))
            {
                users = await GetRegistered(userId);
            }

            if (int.TryParse(user.FindFirst("tenant")?.Value, out int affId))
            {
                affs = await GetVisibility(affId);
            }

            if (page < 1) throw new InvalidOperationException();

            var orig = contests
                .WhereIf(kind.HasValue, c => c.Kind == kind)
                .Where(c => c.IsPublic || users.Contains(c.Id) || affs.Contains(c.Id));

            var total = orig.Count();
            var results = orig
                .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount, users.Contains(c.Id)))
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return new PagedViewList<ContestListModel>(results, page, total, limit);
        }

        public async Task<List<ProblemsetStatistics>> StatisticsAsync(int contestId, int teamId)
        {
            var result = await Db.SubmissionStatistics
                .Where(c => c.ContestId == contestId && c.TeamId == teamId)
                .Join(
                    inner: Db.ContestProblems,
                    outerKeySelector: s => new { s.ContestId, s.ProblemId },
                    innerKeySelector: p => new { p.ContestId, p.ProblemId },
                    resultSelector: (s, p) => new ProblemsetStatistics
                    {
                        AcceptedSubmission = s.AcceptedSubmission,
                        ProblemId = p.ShortName,
                        TotalSubmission = s.TotalSubmission,
                    })
                .ToListAsync();

            result.Sort((a, b) => a.ProblemId.CompareTo(b.ProblemId));
            return result;
        }
    }
}
