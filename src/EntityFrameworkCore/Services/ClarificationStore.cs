using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ClarificationStore<TContext> : IClarificationStore
        where TContext : DbContext
    {
        public TContext Context { get; }

        DbSet<Clarification> Clarifications => Context.Set<Clarification>();

        public ClarificationStore(TContext context)
        {
            Context = context;
        }

        public async Task<Clarification> SendAsync(Clarification clar, Clarification? replyTo)
        {
            var cl = Clarifications.Add(clar);

            if (replyTo != null)
            {
                replyTo.Answered = true;
                Clarifications.Update(replyTo);
            }

            await Context.SaveChangesAsync();
            return cl.Entity;
        }

        public Task<Clarification> FindAsync(Contest contest, int id)
        {
            int cid = contest.Id;
            return Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .SingleOrDefaultAsync();
        }

        public Task<List<Clarification>> ListAsync(
            Contest contest,
            Expression<Func<Clarification, bool>>? predicate)
        {
            int cid = contest.Id;
            return Clarifications
                .Where(c => c.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        public Task<int> CountUnansweredAsync(Contest contest)
        {
            int cid = contest.Id;
            return Clarifications
                .Where(c => c.ContestId == cid && !c.Answered)
                .CountAsync();
        }

        public async Task<bool> SetAnsweredAsync(Contest contest, int id, bool answered)
        {
            int cid = contest.Id;
            return 1 == await Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .BatchUpdateAsync(c => new Clarification { Answered = answered });
        }

        public async Task<bool> ClaimAsync(Contest contest, int id, string jury, bool claim)
        {
            int cid = contest.Id;
            var (from, to) = claim ? (default(string), jury) : (jury, default(string));
            return 1 == await Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .Where(c => c.JuryMember == from)
                .BatchUpdateAsync(c => new Clarification { JuryMember = to });
        }
    }
}
