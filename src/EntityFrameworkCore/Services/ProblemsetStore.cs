using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ProblemsetStore<TContext> : IProblemsetStore
        where TContext : DbContext
    {
        public TContext Context { get; }

        public IProblemStore Problems { get; }

        DbSet<ContestProblem> ContestProblems => Context.Set<ContestProblem>();

        public ProblemsetStore(TContext context, IProblemStore probs)
        {
            Context = context;
            Problems = probs;
        }

        public async Task<IReadOnlyList<ProblemModel>> ListAsync(Contest contest)
        {
            var cid = contest.Id;
            var query =
                from cp in Context.Set<ContestProblem>()
                where cp.ContestId == contest.Id
                join p in Context.Set<Problem>() on cp.ProblemId equals p.Id
                select new ProblemModel(
                    cp.ContestId,
                    cp.ProblemId,
                    cp.ShortName,
                    cp.AllowSubmit,
                    p.AllowJudge,
                    cp.Color,
                    cp.Score,
                    p.Title,
                    p.TimeLimit,
                    p.MemoryLimit,
                    p.CombinedRunCompare,
                    p.Shared);

            var result = await query.ToListAsync();

            var query2 =
                from cp in Context.Set<ContestProblem>()
                where cp.ContestId == cid
                join t in Context.Set<Testcase>() on cp.ProblemId equals t.ProblemId
                group t by cp.ProblemId into g
                select new { g.Key, Count = g.Count(), Score = g.Sum(t => t.Point) };

            var result2 = await query2.ToDictionaryAsync(a => a.Key);

            foreach (var item in result)
            {
                if (result2.TryGetValue(item.ProblemId, out var res))
                {
                    item.TestcaseCount = res.Count;
                    if (item.Score == 0) item.Score = res.Score;
                }

                item.Statement = await Problems.ReadCompiledHtmlAsync(item.ProblemId);
            }

            result.Sort((a, b) => a.ShortName.CompareTo(b.ShortName));
            for (int i = 0; i < result.Count; i++)
                result[i].Rank = i + 1;

            return result;
        }

        public async Task<(bool, string)> CheckAvailabilityAsync(int cid, int pid, int? user)
        {
            if (await ContestProblems.Where(cp => cp.ContestId == cid && cp.ProblemId == pid).AnyAsync())
                return (false, "Problem has been added.");
            IQueryable<Problem> query;

            if (user == null)
                query = Context.Set<Problem>()
                    .Where(p => p.Id == pid);
            else
                query = Context.Set<ProblemAuthor>()
                    .Where(pa => pa.ProblemId == pid && pa.UserId == user)
                    .Join(Context.Set<Problem>(), pa => pa.ProblemId, p => p.Id, (pa, p) => p);

            var prob = await query.FirstOrDefaultAsync();
            if (prob == null)
                return (false, "Problem not found or access denined.");
            return (true, prob.Title);
        }

        public Task CreateAsync(ContestProblem problem)
        {
            ContestProblems.Add(problem);
            return Context.SaveChangesAsync();
        }

        public Task DeleteAsync(ProblemModel problem)
        {
            var (cid, pid) = (problem.ContestId, problem.ProblemId);
            return ContestProblems
                .Where(cp => cp.ContestId == cid && cp.ProblemId == pid)
                .BatchDeleteAsync();
        }

        public Task UpdateAsync(int cid, int pid, Expression<Func<ContestProblem>> change)
        {
            var change2 = Expression.Lambda<Func<ContestProblem, ContestProblem>>(
                change.Body, Expression.Parameter(typeof(ContestProblem), "oldcp"));
            return ContestProblems
                .Where(oldcp => oldcp.ContestId == cid && oldcp.ProblemId == pid)
                .BatchUpdateAsync(change2);
        }

        public Task<List<ContestProblem>> ListByProblemAsync(Problem problem)
        {
            return ContestProblems
                .Where(cp => cp.ProblemId == problem.Id)
                .OrderBy(cp => cp.ContestId)
                .Include(cp => cp.Contest)
                .ToListAsync();
        }
        
        public Task<List<Problem>> RawProblemsAsync(int cid)
        {
            return ContestProblems
                .Where(cp => cp.ContestId == cid)
                .OrderBy(cp => cp.ShortName)
                .Select(cp => cp.Problem)
                .ToListAsync();
        }
    }
}
