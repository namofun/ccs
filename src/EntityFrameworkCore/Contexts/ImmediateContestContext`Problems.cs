using Ccs.Entities;
using Ccs.Models;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        private ProblemCollection? _readed_problem_collection;

        public virtual async Task<ProblemCollection> FetchProblemsAsync(bool nonCached = false)
        {
            if (_readed_problem_collection != null && !nonCached)
                return _readed_problem_collection;

            var res = new ProblemCollection(
                await Ccs.ProblemStore.ListModelsAsync(Contest.Id),
                await Ccs.ProblemStore.ListTestcaseAndScore(Contest.Id));

            for (int i = 0; i < res.Count; i++)
                res[i].Statement = await Polygon.Problems.ReadCompiledHtmlAsync(res[i].ProblemId);

            return _readed_problem_collection = res;
        }

        public virtual Task UpdateProblemAsync(ProblemModel origin, Expression<Func<ContestProblem>> expression)
        {
            return Ccs.ProblemStore.UpdateAsync(origin.ContestId, origin.ProblemId, expression);
        }

        public virtual Task CreateProblemAsync(ContestProblem entity)
        {
            return Ccs.ProblemStore.CreateAsync(entity);
        }

        public virtual Task DeleteProblemAsync(ProblemModel problem)
        {
            return Ccs.ProblemStore.DeleteAsync(problem.ContestId, problem.ProblemId);
        }

        public async Task<List<Statement>> FetchRawStatementsAsync()
        {
            var problems = await FetchProblemsAsync();
            var provider = _services.GetRequiredService<Polygon.Packaging.IStatementProvider>();
            var raw = await Ccs.ProblemStore.RawProblemsAsync(Contest.Id);
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
    }
}
