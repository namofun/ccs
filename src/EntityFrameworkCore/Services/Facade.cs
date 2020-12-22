using Ccs.Services;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs
{
    public class ContestFacade<TContext>
        where TContext : DbContext
    {
        public DbContext Context { get; }

        public IContestStore Contests { get; }

        public IProblemsetStore Problemset { get; }

        public ITeamStore Teams { get; }

        public ISubmissionStore Submissions { get; }

        public ContestFacade(
            TContext context,
            IContestStore store1,
            IProblemsetStore store2,
            ITeamStore store3,
            ISubmissionStore store4)
        {
            Context = context;
            Contests = store1;
            Problemset = store2;
            Teams = store3;
            Submissions = store4;
        }

        public Task<Dictionary<int, int>> StatisticAcceptedAsync(int cid)
        {
            return Context.Set<SubmissionStatistics>()
                .Where(s => s.ContestId == cid && s.AcceptedSubmission > 0)
                .GroupBy(t => t.ProblemId)
                .Select(g => new { ProblemId = g.Key, Count = g.Count() })
                .CachedToDictionaryAsync(
                    k => k.ProblemId, v => v.Count,
                    $"`c{cid}`probs`ac_stat", TimeSpan.FromMinutes(5));
        }
    }
}
