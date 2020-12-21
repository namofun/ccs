using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ContestStore<TContext> : IContestStore
        where TContext : DbContext
    {
        public TContext Context { get; }

        DbSet<Contest> Contests => Context.Set<Contest>();
        
        public ContestStore(TContext context)
        {
            Context = context;
        }

        public Task<Contest> FindAsync(int cid)
        {
            return Contests
                .Where(c => c.Id == cid)
                .SingleOrDefaultAsync();
        }

        public Task<int> MaxEventIdAsync(int cid)
        {
            return Context.Set<Event>()
                .Where(e => e.ContestId == cid)
                .OrderByDescending(e => e.Id)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        public Task UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression)
        {
            return Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(expression);
        }

        public async Task<Contest> CreateAsync(Contest entity)
        {
            var e = Contests.Add(entity);
            await Context.SaveChangesAsync();
            return e.Entity;
        }

        public Task<IPagedList<ContestListModel>> ListAsync(
            int userId, int kind,
            int page = 1, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<ContestListModel>> ListAsync(
            int page = 1, int limit = 100)
        {
            return Contests
                .OrderByDescending(c => c.Id)
                .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTime, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount))
                .ToPagedListAsync(page, limit);
        }

        public Task<List<Event>> FetchEventAsync(int cid, string[]? type = null, int after = 0)
        {
            return Context.Set<Event>()
                .Where(e => e.ContestId == cid && e.Id > after)
                .WhereIf(type != null, e => type.Contains(e.EndpointType))
                .ToListAsync();
        }

        public Task EmitAsync(Event @event)
        {
            Context.Set<Event>().Add(@event);
            return Context.SaveChangesAsync();
        }

        public async Task AssignJuryAsync(Contest contest, IUser user)
        {
            var jury = await Context.Set<Jury>()
                .Where(j => j.ContestId == contest.Id && j.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (jury != null) return;

            Context.Set<Jury>().Add(new Jury
            {
                ContestId = contest.Id,
                UserId = user.Id,
            });

            await Context.SaveChangesAsync();
        }

        public async Task UnassignJuryAsync(Contest contest, IUser user)
        {
            var jury = await Context.Set<Jury>()
                .Where(j => j.ContestId == contest.Id && j.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (jury == null) return;

            Context.Set<Jury>().Remove(jury);
            await Context.SaveChangesAsync();
        }

        public async Task<HashSet<int>> ListJuryAsync(Contest contest)
        {
            var result = new HashSet<int>();
            var query = Context.Set<Jury>().Where(j => j.ContestId == contest.Id);
            await foreach (var item in query.AsAsyncEnumerable())
                result.Add(item.UserId);
            return result;
        }
    }
}
