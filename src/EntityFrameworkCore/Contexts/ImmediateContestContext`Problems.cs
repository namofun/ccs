﻿using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        private ProblemCollection? _readed_problem_collection;

        private IQueryable<ProblemModel> QueryProblems(IQueryable<ContestProblem> contestProblems)
            => from cp in contestProblems
               join p in Db.Problems on cp.ProblemId equals p.Id
               select new ProblemModel(
                   cp.ContestId, cp.ProblemId, cp.ShortName,
                   cp.AllowSubmit, p.AllowJudge,
                   cp.Color, cp.Score, p.TagName, p.Source,
                   p.Title, p.TimeLimit, p.MemoryLimit, p.CombinedRunCompare, p.Shared);

        private IQueryable<ProblemModel> QueryProblems(int cid)
            => QueryProblems(Db.ContestProblems.Where(cp => cp.ContestId == cid));

        private IQueryable<PartialScore> QueryScores(int cid)
            => from cp in Db.ContestProblems
               where cp.ContestId == cid
               join t in Db.Testcases on cp.ProblemId equals t.ProblemId
               group t by cp.ProblemId into g
               select new PartialScore { Id = g.Key, Count = g.Count(), Score = g.Sum(t => t.Point) };

        private Task FixProblemCountAsync(int cid)
            => Db.Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(c => new Contest { ProblemCount =
                    Db.ContestProblems.Count(cp => cp.ContestId == cid) });

        public virtual async Task<ProblemCollection> FetchProblemsAsync(bool nonCached = false)
        {
            if (_readed_problem_collection != null && !nonCached)
                return _readed_problem_collection;

            var res = new ProblemCollection(
                await QueryProblems(Contest.Id).ToListAsync(),
                await QueryScores(Contest.Id).ToDictionaryAsync(k => k.Id, e => (e.Count, e.Score)));

            for (int i = 0; i < res.Count; i++)
                res[i].Statement = await Polygon.Problems.ReadCompiledHtmlAsync(res[i].ProblemId);

            return _readed_problem_collection = res;
        }

        public virtual Task UpdateProblemAsync(
            ProblemModel origin,
            Expression<Func<ContestProblem, ContestProblem>> expression)
        {
            int cid = origin.ContestId, probid = origin.ProblemId;
            return Db.ContestProblems
                .Where(oldcp => oldcp.ContestId == cid && oldcp.ProblemId == probid)
                .BatchUpdateAsync(expression);
        }

        public virtual async Task CreateProblemAsync(ContestProblem entity)
        {
            Db.ContestProblems.Add(entity);
            await Db.SaveChangesAsync();
            await FixProblemCountAsync(Contest.Id);
        }

        public virtual async Task DeleteProblemAsync(ProblemModel problem)
        {
            int cid = problem.ContestId, probid = problem.ProblemId;

            await Db.ContestProblems
                .Where(cp => cp.ContestId == cid && cp.ProblemId == probid)
                .BatchDeleteAsync();

            await FixProblemCountAsync(Contest.Id);
        }

        public virtual async Task<List<Statement>> FetchRawStatementsAsync()
        {
            int cid = Contest.Id;
            var problems = await FetchProblemsAsync();

            var raw = await Db.ContestProblems
                .Where(cp => cp.ContestId == cid)
                .OrderBy(cp => cp.ShortName)
                .Join(Db.Problems, cp => cp.ProblemId, p => p.Id, (cp, p) => p)
                .ToListAsync();

            var provider = _services.GetRequiredService<Polygon.Packaging.IStatementProvider>();

            var stmts = new List<Statement>();
            foreach (var prob in raw)
            {
                var stmt = await provider.ReadAsync(prob);
                stmts.Add(new Statement(prob,
                    stmt.Description, stmt.Input, stmt.Output, stmt.Hint, stmt.Interaction,
                    problems.Find(prob.Id)!.ShortName, stmt.Samples));
            }

            return stmts;
        }

        public async Task<CheckResult> CheckProblemAvailabilityAsync(int probid, ClaimsPrincipal user)
        {
            int cid = Contest.Id;
            int? userId = user.IsInRole("Administrator") ? default(int?) : int.Parse(user.GetUserId() ?? "-110");

            if (await Db.ContestProblems.Where(cp => cp.ContestId == cid && cp.ProblemId == probid).AnyAsync())
                return CheckResult.Fail("Problem has been added.");

            IQueryable<Problem> query;
            if (userId == null)
                query = Db.Problems
                    .Where(p => p.Id == probid);
            else
                query = Db.ProblemAuthors
                    .Where(pa => pa.ProblemId == probid && pa.UserId == userId)
                    .Join(Db.Problems, pa => pa.ProblemId, p => p.Id, (pa, p) => p);

            var prob = await query.FirstOrDefaultAsync();
            if (prob == null)
                return CheckResult.Fail("Problem not found or access denined.");

            return CheckResult.Succeed(prob.Title);
        }

        public virtual async Task<IFileInfo?> FetchTestcaseAsync(ProblemModel problem, int testcaseId, string filetype)
        {
            var testcase = await Polygon.Testcases.FindAsync(testcaseId, problem.ProblemId);
            if (testcase == null || (!problem.Shared && testcase.IsSecret)) return null;
            return await Polygon.Testcases.GetFileAsync(testcase, filetype);
        }
    }
}
