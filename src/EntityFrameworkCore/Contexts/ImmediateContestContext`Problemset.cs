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
    public partial class ImmediateContestContext : IProblemsetContext
    {
        public async Task<ProblemModel?> FindProblemsetAsync(string probid, bool withStatement)
        {
            int cid = Contest.Id;
            var model = await QueryProblems(Db.ContestProblems
                .Where(cp => cp.ContestId == cid && cp.ShortName == probid)
                .OrderBy(cp => cp.ShortName))
                .SingleOrDefaultAsync();

            if (!withStatement || model == null) return model;
            model.Statement = await Polygon.Problems.ReadCompiledHtmlAsync(model.ProblemId);
            return model;
        }

        public virtual async Task<IPagedList<ProblemModel>> ListProblemsAsync(int page, int count)
        {
            int totalCount = _contest.ProblemCount;
            int cid = Contest.Id;

            var model = await QueryProblems(Db.ContestProblems
                .Where(cp => cp.ContestId == cid)
                .OrderBy(cp => cp.ShortName))
                .ToListAsync();

            return new PagedViewList<ProblemModel>(model, page, totalCount, count);
        }

        public virtual Task<IPagedList<TSolution>> ListSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            Expression<Func<Submission, bool>> predicate,
            int page, int perpage)
        {
            return Polygon.Submissions.ListWithJudgingAsync((page, perpage), selector, predicate);
        }
    }
}
