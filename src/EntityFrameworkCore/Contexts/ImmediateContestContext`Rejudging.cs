using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual Task<Rejudging> CreateRejudgingAsync(Rejudging entity)
        {
            return Polygon.Rejudgings.CreateAsync(entity);
        }

        public virtual Task DeleteRejudgingAsync(Rejudging entity)
        {
            return Polygon.Rejudgings.DeleteAsync(entity);
        }

        public virtual Task<Rejudging> FindRejudgingAsync(int id)
        {
            return Polygon.Rejudgings.FindAsync(Contest.Id, id);
        }

        public virtual Task<List<Rejudging>> FetchRejudgingsAsync(bool includeStat = true)
        {
            return Polygon.Rejudgings.ListAsync(Contest.Id, includeStat);
        }

        public virtual Task<List<Judgehost>> FetchJudgehostsAsync()
        {
            return Polygon.Judgehosts.ListAsync();
        }

        public virtual Task<IEnumerable<RejudgingDifference>> ViewRejudgingAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter = null)
        {
            return Polygon.Rejudgings.ViewAsync(rejudge, filter);
        }

        public virtual Task<int> BatchRejudgeAsync(Expression<Func<Submission, Judging, bool>> predicate, Rejudging? rejudge = null, bool fullTest = false)
        {
            return Polygon.Rejudgings.BatchRejudgeAsync(predicate, rejudge, fullTest);
        }

        public virtual Task CancelRejudgingAsync(Rejudging rejudge, int uid)
        {
            return Polygon.Rejudgings.CancelAsync(rejudge, uid);
        }

        public virtual Task ApplyRejudgingAsync(Rejudging rejudge, int uid)
        {
            return Polygon.Rejudgings.ApplyAsync(rejudge, uid);
        }
    }
}
