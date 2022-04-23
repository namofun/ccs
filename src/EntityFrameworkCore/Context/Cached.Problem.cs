using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    public partial class CachedContestContext
    {
        public override Task<ProblemCollection> ListProblemsAsync(bool nonCached = false)
        {
            if (Contest.Feature == CcsDefaults.KindProblemset)
            {
                Logger.ImproperCall("ListProblemsAsync(bool) -> Task<ProblemCollection>", Contest);
            }

            if (nonCached)
            {
                return base.ListProblemsAsync(true);
            }

            return CacheAsync("Problems", _options.Problem,
                async () => await base.ListProblemsAsync(true));
        }

        public override async Task<ProblemModel?> FindProblemAsync(int probid, bool withStatement = false)
        {
            if (Contest.Feature == CcsDefaults.KindProblemset)
            {
                return await base.FindProblemAsync(probid, withStatement);
            }
            else
            {
                var probs = await ListProblemsAsync();
                var model = probs.Find(probid);
                if (model != null && withStatement)
                {
                    await LoadStatementAsync(model);
                }

                return model;
            }
        }

        public override async Task<ProblemModel?> FindProblemAsync(string probid, bool withStatement = false)
        {
            if (Contest.Feature == CcsDefaults.KindProblemset)
            {
                return await base.FindProblemAsync(probid, withStatement);
            }
            else
            {
                var probs = await ListProblemsAsync();
                var model = probs.Find(probid);
                if (model != null && withStatement)
                {
                    await LoadStatementAsync(model);
                }

                return model;
            }
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
