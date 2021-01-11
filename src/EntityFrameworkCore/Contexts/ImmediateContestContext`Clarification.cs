using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate)
        {
            return Ccs.ClarificationStore.ListAsync(Contest.Id, predicate);
        }

        public virtual Task<Clarification> FindClarificationAsync(int id)
        {
            return Ccs.ClarificationStore.FindAsync(Contest.Id, id);
        }

        public virtual Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null)
        {
            return Ccs.ClarificationStore.SendAsync(clar, replyTo);
        }

        public virtual Task<bool> SetClarificationAnsweredAsync(int id, bool answered)
        {
            return Ccs.ClarificationStore.SetAnsweredAsync(Contest.Id, id, answered);
        }

        public virtual Task<bool> ClaimClarificationAsync(int id, string jury, bool claim)
        {
            return Ccs.ClarificationStore.ClaimAsync(Contest.Id, id, jury, claim);
        }
    }
}
