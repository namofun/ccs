﻿using Ccs.Entities;
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
        [Checked]
        public virtual Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate)
        {
            int cid = Contest.Id;
            return Db.Clarifications
                .Where(c => c.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        [Checked]
        public virtual Task<Clarification> FindClarificationAsync(int id)
        {
            int cid = Contest.Id;
            return Db.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .SingleOrDefaultAsync();
        }

        [Checked]
        public virtual async Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null)
        {
            var cl = Db.Clarifications.Add(clar);

            if (replyTo != null)
            {
                replyTo.Answered = true;
                Db.Clarifications.Update(replyTo);
            }

            await Db.SaveChangesAsync();
            return cl.Entity;
        }

        [Checked]
        public virtual async Task<bool> SetClarificationAnsweredAsync(int id, bool answered)
        {
            int cid = Contest.Id;
            return 1 == await Db.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .BatchUpdateAsync(c => new Clarification { Answered = answered });
        }

        [Checked]
        public virtual async Task<bool> ClaimClarificationAsync(int id, string jury, bool claim)
        {
            int cid = Contest.Id;
            var (from, to) = claim ? (default(string), jury) : (jury, default(string));
            return 1 == await Db.Clarifications
                .Where(c => c.ContestId == cid && c.Id == id)
                .Where(c => c.JuryMember == from)
                .BatchUpdateAsync(c => new Clarification { JuryMember = to });
        }
    }
}