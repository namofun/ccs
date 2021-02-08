using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IRejudgingContext
    {
        Task<Rejudging> IRejudgingContext.CreateAsync(Rejudging entity)
        {
            return Polygon.Rejudgings.CreateAsync(entity);
        }

        Task IRejudgingContext.DeleteAsync(Rejudging entity)
        {
            return Polygon.Rejudgings.DeleteAsync(entity);
        }

        Task<Rejudging?> IRejudgingContext.FindAsync(int id)
        {
            return Polygon.Rejudgings.FindAsync(Contest.Id, id)!;
        }

        Task<List<Rejudging>> IRejudgingContext.ListAsync(bool includeStat)
        {
            return Polygon.Rejudgings.ListAsync(Contest.Id, includeStat);
        }

        Task<List<Judgehost>> IRejudgingContext.GetJudgehostsAsync()
        {
            return Polygon.Judgehosts.ListAsync();
        }

        Task<IEnumerable<RejudgingDifference>> IRejudgingContext.ViewAsync(
            Rejudging rejudge,
            Expression<Func<Judging, Judging, Submission, bool>>? filter)
        {
            return Polygon.Rejudgings.ViewAsync(rejudge, filter);
        }

        Task<int> IRejudgingContext.RejudgeAsync(
            Expression<Func<Submission, Judging, bool>> predicate,
            Rejudging? rejudge, bool fullTest)
        {
            return Polygon.Rejudgings.BatchRejudgeAsync(predicate, rejudge, fullTest);
        }

        Task IRejudgingContext.CancelAsync(Rejudging rejudge, int uid)
        {
            return Polygon.Rejudgings.CancelAsync(rejudge, uid);
        }

        Task IRejudgingContext.ApplyAsync(Rejudging rejudge, int uid)
        {
            return Polygon.Rejudgings.ApplyAsync(rejudge, uid);
        }
    }
}
