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
    public class PrintingService<TContext> : IPrintingService, ISupportDbContext
        where TContext : DbContext, IContestDbContext
    {
        public IContestDbContext Db { get; }

        public PrintingService(TContext context)
            => Db = context;

        public Task<List<PrintingTask>> ListAsync(int contestId, int limit)
        {
            var query =
                from p in Db.Printings
                where p.ContestId == contestId
                orderby p.Id descending
                join u in Db.Users on p.UserId equals u.Id
                into uu from u in uu.DefaultIfEmpty()
                join tu in Db.TeamMembers on new { p.ContestId, p.UserId } equals new { tu.ContestId, tu.UserId }
                into tuu from tu in tuu.DefaultIfEmpty()
                join t in Db.Teams on new { tu.ContestId, tu.TeamId } equals new { t.ContestId, t.TeamId }
                into tt from t in tt.DefaultIfEmpty()
                select new PrintingTask
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    Language = p.LanguageId,
                    Done = p.Done,
                    Location = t.Location,
                    Time = p.Time,
                    TeamName = tu == null
                        ? $"u{u.Id} - {u.UserName}"
                        : $"t{tu.TeamId} - {t.TeamName}",
                };

            return query.Take(limit).ToListAsync();
        }

        public async Task<Printing> CreateAsync(Printing entity)
        {
            var e = Db.Printings.Add(entity);
            await Db.SaveChangesAsync();
            return e.Entity;
        }

        public async Task<bool> SetStateAsync(Printing entity, bool? done)
        {
            int id = entity.Id;
            return 1 == await Db.Printings
                .Where(p => p.Id == id)
                .BatchUpdateAsync(p => new Printing { Done = done });
        }

        public Task<Printing?> FindAsync(int id, bool full)
        {
            var query = Db.Printings.Where(p => p.Id == id);

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
            return Db.Printings
                .Where(condition)
                .FirstOrDefaultAsync()!;
        }
    }
}
