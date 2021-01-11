using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate)
        {
            int cid = Contest.Id;
            return Ccs.Clarifications
                .Where(c => c.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        public virtual Task<Clarification> FindClarificationAsync(int id)
        {
            int cid = Contest.Id;
            return Ccs.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .SingleOrDefaultAsync();
        }

        public virtual async Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null)
        {
            var cl = Ccs.Clarifications.Add(clar);

            if (replyTo != null)
            {
                replyTo.Answered = true;
                Ccs.Clarifications.Update(replyTo);
            }

            await Ccs.SaveChangesAsync();
            return cl.Entity;
        }

        public virtual async Task<bool> SetClarificationAnsweredAsync(int id, bool answered)
        {
            int cid = Contest.Id;
            return 1 == await Ccs.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .BatchUpdateAsync(c => new Clarification { Answered = answered });
        }

        public virtual async Task<bool> ClaimClarificationAsync(int id, string jury, bool claim)
        {
            int cid = Contest.Id;
            var (from, to) = claim ? (default(string), jury) : (jury, default(string));
            return 1 == await Ccs.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .Where(c => c.JuryMember == from)
                .BatchUpdateAsync(c => new Clarification { JuryMember = to });
        }
    }
}
