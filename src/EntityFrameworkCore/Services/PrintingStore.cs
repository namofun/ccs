using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class PrintingStore<TContext> : IPrintingStore
        where TContext : DbContext
    {
        public TContext Context { get; }

        DbSet<Printing> Printings => Context.Set<Printing>();

        public PrintingStore(TContext context)
        {
            Context = context;
        }

        public Task<List<T>> ListAsync<T>(
            int takeCount, int page,
            Expression<Func<Printing, object, Team, T>> expression,
            Expression<Func<Printing, bool>>? predicate)
        {
            throw new NotImplementedException();
            /*
            IQueryable<Printing> prints = Printings;
            if (predicate != null) prints = prints.Where(predicate);

            var query =
                from p in prints
                join u in Context.Set<object>() on p.UserId equals u.Id
                into uu from u in uu.DefaultIfEmpty()
                join tu in Members on new { p.ContestId, p.UserId } equals new { tu.ContestId, tu.UserId }
                into tuu from tu in tuu.DefaultIfEmpty()
                select new { p, u, t = tu.Team };

            if (page > 0)
            {
                query = query.OrderByDescending(a => a.p.Time);
            }
            else
            {
                query = query.OrderBy(a => a.p.Time);
                page = -page;
            }

            var selector = expression.Combine(
                objectTemplate: new { p = (Printing)null, u = (User)null, t = (Team)null },
                place1: a => a.p, place2: a => a.u, place3: a => a.t);
            return query.Select(selector)
                .Skip((page - 1) * takeCount).Take(takeCount)
                .ToListAsync();
            */
        }

        public async Task<Printing> CreateAsync(Printing entity)
        {
            var e = Printings.Add(entity);
            await Context.SaveChangesAsync();
            return e.Entity;
        }

        public async Task<bool> SetStateAsync(Printing entity, bool? done)
        {
            int id = entity.Id;
            return 1 == await Printings
                .Where(p => p.Id == id)
                .BatchUpdateAsync(p => new Printing { Done = done });
        }

        public Task<Printing?> FindAsync(int id)
        {
            return Printings
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync()!;
        }

        public Task<Printing?> FirstAsync(Expression<Func<Printing, bool>> condition)
        {
            return Printings
                .Where(condition)
                .FirstOrDefaultAsync()!;
        }
    }
}
