using Microsoft.EntityFrameworkCore;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Contesting.Services
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
            Rejudging rejudging,
            bool fullTest)
        {
            return Polygon.Rejudgings.BatchRejudgeAsync(predicate, rejudging, fullTest);
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
            {
                return CheckResult<Rejudging>.Fail("Contest should be ended first.");
            }

            if (Contest.Settings.SystemTestRejudgingId != null)
            {
                var rej = await Polygon.Rejudgings.FindAsync(Contest.Id, Contest.Settings.SystemTestRejudgingId.Value);
                return rej != null
                    ? CheckResult<Rejudging>.Succeed(rej)
                    : CheckResult<Rejudging>.Fail($"Rejudging {Contest.Settings.SystemTestRejudgingId.Value} not found.");
            }

            if (await Polygon.Rejudgings.CountUndoneAsync(Contest.Id) != 0)
            {
                return CheckResult<Rejudging>.Fail("There's pending rejudgings.");
            }

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

            var settings = Contest.Settings.Clone();
            settings.SystemTestRejudgingId = rejudging.Id;
            var settingsJson = settings.ToJson();
            await UpdateContestAsync(c => new Entities.Contest { SettingsJson = settingsJson });

            var startTime = Contest.StartTime!.Value;
            var endTime = (Contest.StartTime + Contest.EndTime)!.Value;
            int count = await Polygon.Rejudgings.BatchRejudgeAsync(
                (s, j) => j.Status == Verdict.Accepted && s.Time >= startTime && s.Time <= endTime,
                rejudging, immediateApply: true, stageAsRunning: true);

            if (count == 0)
            {
                await Polygon.Rejudgings.DeleteAsync(rejudging);
                return CheckResult<Rejudging>.Fail("There's no accepted submissions in this contest.");
            }

            await Mediator.Publish(new Events.ScoreboardRefreshEvent(this));

            await Db.Judgings
                .Where(j => j.RejudgingId == rejudging.Id)
                .BatchUpdateAsync(j => new Judging { Status = Verdict.Pending });

            return CheckResult<Rejudging>.Succeed(rejudging);
        }

        async Task IRejudgingContext.ApplyRatingChangesAsync()
        {
            var ratingUpdater = Get<IRatingUpdater>();
            var conf = Get<IConfigurationRegistry>();

            var lastTime = await conf.GetDateTimeOffsetAsync(CcsDefaults.ConfigurationLastRatingChangeTime);
            if (lastTime > Contest.StartTime) throw new InvalidOperationException();

            await ratingUpdater.ApplyAsync(Contest);
            var settings = Contest.Settings.Clone();
            settings.RatingChangesApplied = true;
            var settingsJson = settings.ToJson();
            await UpdateContestAsync(c => new Entities.Contest { SettingsJson = settingsJson });
            await conf.UpdateAsync(CcsDefaults.ConfigurationLastRatingChangeTime, Contest.StartTime.ToJson());
        }

        async Task IRejudgingContext.RollbackRatingChangesAsync()
        {
            var ratingUpdater = Get<IRatingUpdater>();
            var conf = Get<IConfigurationRegistry>();

            var lastTime = await conf.GetDateTimeOffsetAsync(CcsDefaults.ConfigurationLastRatingChangeTime);
            if (lastTime != Contest.StartTime) throw new InvalidOperationException();

            await ratingUpdater.RollbackAsync(Contest);
            var settings = Contest.Settings.Clone();
            settings.RatingChangesApplied = null;
            var settingsJson = settings.ToJson();
            await UpdateContestAsync(c => new Entities.Contest { SettingsJson = settingsJson });
        }
    }
}
