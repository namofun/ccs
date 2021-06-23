using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        async Task<CheckResult<Rejudging>> IRejudgingContext.SystemTestAsync(int uid)
        {
            if (Contest.GetState() < Entities.ContestState.Ended)
                return CheckResult<Rejudging>.Fail("Contest should be ended first.");
            if (Contest.Settings.SystemTestRejudgingId != null)
                return CheckResult<Rejudging>.Succeed(await Polygon.Rejudgings.FindAsync(Contest.Id, Contest.Settings.SystemTestRejudgingId.Value));
            if (await Polygon.Rejudgings.CountUndoneAsync(Contest.Id) != 0)
                return CheckResult<Rejudging>.Fail("There's pending rejudgings.");

            var rejudging = new Rejudging
            {
                ContestId = Contest.Id,
                Reason = "System Test",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now,
                IssuedBy = uid,
                OperatedBy = uid,
                Applied = true,
            };

            rejudging = await Polygon.Rejudgings.CreateAsync(rejudging);
            int cid = rejudging.ContestId, rid = rejudging.Id;
            var locks = Db.Submissions
                .Where(s => s.ContestId == cid)
                .Join(
                    inner: Db.Judgings,
                    outerKeySelector: s => new { SubmissionId = s.Id, Active = true },
                    innerKeySelector: j => new { j.SubmissionId, j.Active },
                    resultSelector: (s, j) => new { s, j })
                .Where(a => a.j.Status == Verdict.Accepted)
                .Select(s => s.s.Id);

            int count = await Db.Submissions
                .Where(s => locks.Contains(s.Id))
                .BatchUpdateAsync(s => new Submission { RejudgingId = rid });

            if (count == 0)
            {
                await Polygon.Rejudgings.DeleteAsync(rejudging);
                return CheckResult<Rejudging>.Fail("There's no accepted submissions in this contest.");
            }

            locks = Db.Submissions
                .Where(s => s.RejudgingId == rid)
                .Select(s => s.Id);

            int inserts = await Db.Submissions
                .Where(s => s.RejudgingId == rid)
                .Join(
                    inner: Db.Judgings,
                    outerKeySelector: s => new { SubmissionId = s.Id, Active = true },
                    innerKeySelector: j => new { j.SubmissionId, j.Active },
                    resultSelector: (s, j) => new Judging
                    {
                        Active = false,
                        SubmissionId = s.Id,
                        Status = Verdict.Running,
                        RejudgingId = rid,
                        PreviousJudgingId = j.Id,
                        FullTest = j.FullTest,
                    })
                .BatchInsertIntoAsync((DbSet<Judging>)Db.Judgings);

            if (count != inserts) throw new ArgumentException("Error database state.");

            await Db.Judgings
                .Where(j => locks.Contains(j.SubmissionId) && (j.Active || j.RejudgingId == rid))
                .BatchUpdateAsync(j => new Judging { Active = j.RejudgingId == rid });

            var settings = Contest.Settings.Clone();
            settings.SystemTestRejudgingId = rejudging.Id;
            var settingsJson = settings.ToJson();
            await UpdateContestAsync(c => new Entities.Contest { SettingsJson = settingsJson });

            await Mediator.Publish(new Events.ScoreboardRefreshEvent(this));

            await Db.Judgings
                .Where(j => j.RejudgingId == rid)
                .BatchUpdateAsync(j => new Judging { Status = Verdict.Pending });

            return CheckResult<Rejudging>.Succeed(rejudging);
        }
    }
}
