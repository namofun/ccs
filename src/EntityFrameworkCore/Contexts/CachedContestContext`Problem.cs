using Ccs.Entities;
using Ccs.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CachedContestContext
    {
        public override Task<ProblemCollection> ListProblemsAsync(bool nonCached = false)
        {
            if (nonCached)
                return base.ListProblemsAsync(true);

            return CacheAsync("Problems", _options.Problem,
                async () => await base.ListProblemsAsync(true));
        }

        public override async Task UpdateProblemAsync(
            ProblemModel origin,
            Expression<Func<ContestProblem, ContestProblem>> expression)
        {
            await base.UpdateProblemAsync(origin, expression);
            Expire("Problems");
        }

        public override async Task CreateProblemAsync(ContestProblem entity)
        {
            await base.CreateProblemAsync(entity);
            Expire("Problems");
        }

        public override async Task DeleteProblemAsync(ProblemModel problem)
        {
            await base.DeleteProblemAsync(problem);
            Expire("Problems");
        }
    }
}
