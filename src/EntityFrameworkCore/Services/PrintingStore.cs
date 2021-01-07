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
    public class PrintingStore<TUser, TContext> : IPrintingService
        where TUser : SatelliteSite.IdentityModule.Entities.User
        where TContext : DbContext
    {
        public TContext Context { get; }

        DbSet<Printing> Printings => Context.Set<Printing>();

        public PrintingStore(TContext context)
        {
            Context = context;
        }

        public Task<List<PrintingTask>> ListAsync(int contestId, int limit)
        {
            var query =
                from p in Printings
                where p.ContestId == contestId
                orderby p.Id descending
                join u in Context.Set<TUser>() on p.UserId equals u.Id
                into uu from u in uu.DefaultIfEmpty()
                join tu in Context.Set<Member>() on new { p.ContestId, p.UserId } equals new { tu.ContestId, tu.UserId }
                into tuu from tu in tuu.DefaultIfEmpty()
                select new PrintingTask
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    Language = p.LanguageId,
                    Done = p.Done,
                    Location = tu.Team.Location,
                    Time = p.Time,
                    TeamName = tu == null
                        ? $"u{u.Id} - {u.UserName}"
                        : $"t{tu.TeamId} - {tu.Team.TeamName}",
                };

            return query.Take(limit).ToListAsync();
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

        public Task<Printing?> FindAsync(int id, bool full)
        {
            var query = Printings.Where(p => p.Id == id);

            if (!full)
            {
                query = query.Select(p => new Printing
                {
                    ContestId = p.ContestId,
                    Done = p.Done,
                    FileName = p.FileName,
                    Id = p.Id,
                    LanguageId = p.LanguageId,
                    Time = p.Time,
                    UserId = p.UserId,
                });
            }

            return query.SingleOrDefaultAsync()!;
        }

        public Task<Printing?> FirstAsync(Expression<Func<Printing, bool>> condition)
        {
            return Printings
                .Where(condition)
                .FirstOrDefaultAsync()!;
        }
    }
}
