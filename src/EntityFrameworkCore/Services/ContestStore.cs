using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
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
                .CachedSingleOrDefaultAsync($"`c{cid}`info", TimeSpan.FromMinutes(5));
        }

        public Task<int> MaxEventIdAsync(int cid)
        {
            return Context.Set<Event>()
                .Where(e => e.ContestId == cid)
                .OrderByDescending(e => e.Id)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression)
        {
            int solved = await Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(expression);

            Context.RemoveCacheEntry($"`c{cid}`info");
            Context.RemoveCacheEntry($"`c{cid}`internal_state");
            Context.RemoveCacheEntry($"cont::list");
            Context.RemoveCacheEntry($"gym::list");
        }

        public async Task<Contest> CreateAsync(Contest entity)
        {
            var e = Contests.Add(entity);
            await Context.SaveChangesAsync();
            return e.Entity;
        }

        public Task<IPagedList<ContestListModel>> ListAsync(int userId, int kind, int page = 1, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<ContestListModel>> ListAsync(int page = 1, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> FetchEventAsync(int cid, int after = 0)
        {
            return Context.Set<Event>()
                .Where(e => e.ContestId == cid && e.Id > after)
                .ToListAsync();
        }

        public Task EmitAsync(Event @event)
        {
            Context.Set<Event>().Add(@event);
            return Context.SaveChangesAsync();
        }
    }
}
