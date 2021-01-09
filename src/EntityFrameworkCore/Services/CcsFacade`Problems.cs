using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : IProblemsetStore
    {
        private static readonly ParameterExpression _oldcp
            = Expression.Parameter(typeof(ContestProblem), "oldcp");

        async Task<IReadOnlyList<ProblemModel>> IProblemsetStore.ListAsync(int cid)
        {
            var query =
                from cp in ContestProblems
                where cp.ContestId == cid
                join p in Problems on cp.ProblemId equals p.Id
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
                from cp in ContestProblems
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
            }

            result.Sort((a, b) => a.ShortName.CompareTo(b.ShortName));
            for (int i = 0; i < result.Count; i++)
                result[i].Rank = i + 1;

            return result;
        }

        async Task<CheckResult> IProblemsetStore.CheckAvailabilityAsync(int cid, int probid, int? user)
        {
            if (await ContestProblems.Where(cp => cp.ContestId == cid && cp.ProblemId == probid).AnyAsync())
                return CheckResult.Fail("Problem has been added.");

            IQueryable<Problem> query;
            if (user == null)
                query = Problems
                    .Where(p => p.Id == probid);
            else
                query = Context.Set<ProblemAuthor>()
                    .Where(pa => pa.ProblemId == probid && pa.UserId == user)
                    .Join(Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p);

            var prob = await query.FirstOrDefaultAsync();
            if (prob == null)
                return CheckResult.Fail("Problem not found or access denined.");

            return CheckResult.Succeed(prob.Title);
        }

        Task IProblemsetStore.CreateAsync(ContestProblem problem)
        {
            ContestProblems.Add(problem);
            return Context.SaveChangesAsync();
        }

        Task IProblemsetStore.DeleteAsync(int cid, int probid)
        {
            return ContestProblems
                .Where(cp => cp.ContestId == cid && cp.ProblemId == probid)
                .BatchDeleteAsync();
        }

        Task IProblemsetStore.UpdateAsync(int cid, int probid, Expression<Func<ContestProblem>> change)
        {
            return ContestProblems
                .Where(oldcp => oldcp.ContestId == cid && oldcp.ProblemId == probid)
                .BatchUpdateAsync(Expression.Lambda<Func<ContestProblem, ContestProblem>>(change.Body, _oldcp));
        }

        async Task<IReadOnlyList<ProblemModel>> IProblemsetStore.ListByProblemAsync(int probid)
        {
            var query =
                from cp in ContestProblems
                where cp.ProblemId == probid
                join c in Contests on cp.ContestId equals c.Id
                select new ProblemModel(
                    cp.ContestId,
                    cp.ProblemId,
                    cp.ShortName,
                    cp.AllowSubmit,
                    cp.Color,
                    cp.Score,
                    c.Name,
                    c.Kind,
                    c.RankingStrategy);

            return await query.ToListAsync();
        }

        Task<List<Problem>> IProblemsetStore.RawProblemsAsync(int cid)
        {
            return ContestProblems
                .Where(cp => cp.ContestId == cid)
                .OrderBy(cp => cp.ShortName)
                .Join(Problems, cp => cp.ProblemId, p => p.Id, (cp, p) => p)
                .ToListAsync();
        }
    }
}
