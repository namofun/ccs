using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : IClarificationStore
    {
        async Task<Clarification> IClarificationStore.SendAsync(Clarification clar, Clarification? replyTo)
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

        Task<Clarification> IClarificationStore.FindAsync(int cid, int id)
        {
            return Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .SingleOrDefaultAsync();
        }

        Task<List<Clarification>> IClarificationStore.ListAsync(int cid, Expression<Func<Clarification, bool>>? predicate)
        {
            return Clarifications
                .Where(c => c.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        Task<int> IClarificationStore.CountUnansweredAsync(int cid)
        {
            return Clarifications
                .Where(c => c.ContestId == cid && !c.Answered)
                .CountAsync();
        }

        async Task<bool> IClarificationStore.SetAnsweredAsync(int cid, int id, bool answered)
        {
            return 1 == await Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .BatchUpdateAsync(c => new Clarification { Answered = answered });
        }

        async Task<bool> IClarificationStore.ClaimAsync(int cid, int id, string jury, bool claim)
        {
            var (from, to) = claim ? (default(string), jury) : (jury, default(string));
            return 1 == await Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .Where(c => c.JuryMember == from)
                .BatchUpdateAsync(c => new Clarification { JuryMember = to });
        }
    }
}
