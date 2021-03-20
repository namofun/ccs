using Ccs.Models;
using Ccs.Services;
using Ccs.Specifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContestState = Ccs.Entities.ContestState;
using Verdict = Polygon.Entities.Verdict;

namespace SatelliteSite.ContestModule
{
    public class ResetEventResult : LongRunningOperationResult, EventBatch.ILoggable
    {
        private readonly Stopwatch _stopwatch;

        public ResetEventResult() : base("text/html")
        {
            _stopwatch = new Stopwatch();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var ctx = GetContext<IContestContext>();

            await WriteAsync("<pre>");

            _stopwatch.Start();
            // clean up
            await ((IJuryContext)ctx).CleanEventsAsync();
            await LogAsync("Old events cleared.\n");

            int cid = ctx.Contest.Id;
            var deadline = DateTimeOffset.Now;
            var contestTime = ctx.Contest.StartTime ?? deadline;
            if (contestTime > deadline) contestTime = deadline;

            var submits = await((ISubmissionContext)ctx).ListSolutionsAsync((s, j) => s, s => !s.Ignored);
            var earlyTime = submits.Select(s => s.Time).DefaultIfEmpty(contestTime).Min();

            using (var batch = new EventBatch(cid, earlyTime.AddDays(-7), this))
            {
                // contests
                batch.AddCreate(new Contest(ctx.Contest));

                await ctx.EmitEventAsync(batch);
            }

            IReadOnlyDictionary<int, Tenant.Entities.Affiliation> affs;
            using (var batch = new EventBatch(cid, earlyTime.AddDays(-5), this))
            {
                // judgement-types
                batch.AddCreate(JudgementType.Defaults);

                // languages
                var langs = await ctx.ListLanguagesAsync();
                batch.AddCreate(langs, l => new Language(l));

                // groups
                var cats = await ctx.ListCategoriesAsync();
                batch.AddCreate(cats, c => new Group(c.Value));

                // organizations
                affs = await ctx.ListAffiliationsAsync();
                batch.AddCreate(affs, a => new Organization(a.Value));

                // problems
                var probs = await ctx.ListProblemsAsync();
                batch.AddCreate(probs, p => new Problem(p));

                await ctx.EmitEventAsync(batch);
            }

            using (var batch = new EventBatch(cid, earlyTime.AddDays(-3), this))
            {
                // teams
                var teams = await((ITeamContext)ctx).ListTeamsAsync(t => t.Status == 1);
                batch.AddCreate(teams, t => new Team(t, affs[t.AffiliationId]));

                await ctx.EmitEventAsync(batch);
            }

            using (var batch = new EventBatch(cid, deadline, this))
            {
                // clarifications
                var clars = await((IClarificationContext)ctx).ListClarificationsAsync(c => c.SubmitTime <= deadline);
                batch.AddCreate(clars, c => new Clarification(c, contestTime));

                // submissions
                var judgings = await((ISubmissionContext)ctx).ListJudgingsAsync(j => j.Status != Verdict.Pending);
                var runs = await((ISubmissionContext)ctx).ListJudgingRunsAsync();
                var testcases = (await((IProblemContext)ctx).ListTestcasesAsync()).ToDictionary(t => t.Id);

                batch.AddCreate(submits, s => new Submission(s, contestTime));
                batch.AddCreate(runs, r => new Run(r, contestTime, testcases[r.TestcaseId].Rank));
                batch.AddCreate(judgings, j => new Judgement(j, contestTime, Verdict.Running));
                batch.AddUpdate(judgings.Where(j => j.Status != Verdict.Running), j => new Judgement(j, contestTime));

                var state = ctx.Contest.GetState(deadline);
                if (state >= ContestState.Started) batch.AddUpdate(new State(ctx.Contest, ctx.Contest.StartTime.Value));
                if (state >= ContestState.Frozen && ctx.Contest.FreezeTime.HasValue) batch.AddUpdate(new State(ctx.Contest, (ctx.Contest.StartTime + ctx.Contest.FreezeTime).Value));
                if (state >= ContestState.Ended) batch.AddUpdate(new State(ctx.Contest, (ctx.Contest.StartTime + ctx.Contest.EndTime).Value));
                if (state >= ContestState.Finalized && ctx.Contest.UnfreezeTime.HasValue) batch.AddUpdate(new State(ctx.Contest, (ctx.Contest.StartTime + ctx.Contest.UnfreezeTime).Value));

                await ctx.EmitEventAsync(batch);
            }

            await WriteAsync("</pre>");
        }

        public Task LogAsync(string message)
        {
            return WriteAsync($"[ {_stopwatch.Elapsed.TotalSeconds:F3} ] {message}\n");
        }
    }
}
