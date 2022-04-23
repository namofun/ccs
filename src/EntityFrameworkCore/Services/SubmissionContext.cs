using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;
using Xylab.Polygon.Storages;

namespace Xylab.Contesting.Services
{
    public partial class ImmediateContestContext : ISubmissionContext
    {
        public virtual Task ToggleIgnoreAsync(Submission submission, bool ignore)
        {
            if (submission.ContestId != Contest.Id) throw new InvalidOperationException();
            return Polygon.Submissions.UpdateAsync(submission, s => new Submission { Ignored = ignore });
        }

        public virtual async Task<Submission?> FindSubmissionAsync(int submissionId, bool includeJudgings = false)
        {
            var result = await Polygon.Submissions.FindAsync(submissionId, includeJudgings);
            return result?.ContestId == Contest.Id ? result : null;
        }

        public virtual Task<List<T>> ListSubmissionsAsync<T>(
            Expression<Func<Submission, T>> projection,
            Expression<Func<Submission, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return Polygon.Submissions.ListAsync(projection, predicate.Combine(s => s.ContestId == cid));
        }

        public virtual Task<List<Solution>> ListSolutionsAsync(int? probid = null, string? langid = null, int? teamid = null, bool all = false)
        {
            int cid = Contest.Id;
            var cond = Expr
                .Of<Submission>(s => s.ContestId == cid)!
                .CombineIf(probid.HasValue, s => s.ProblemId == probid)
                .CombineIf(teamid.HasValue, s => s.TeamId == teamid)
                .CombineIf(!string.IsNullOrEmpty(langid), s => s.Language == langid);
            int? limit = all ? default(int?) : 75;

            return Polygon.Submissions.ListWithJudgingAsync(cond, true, limit);
        }

        public virtual Task<IPagedList<Solution>> ListSolutionsAsync(int page, int perPage)
        {
            int cid = Contest.Id;
            return Polygon.Submissions.ListWithJudgingAsync((page, perPage), s => s.ContestId == cid);
        }

        public Task<IPagedList<Solution>> ListSolutionsAsync(
            int page, int perPage,
            int? probid = null, string? langid = null, int? teamid = null, Verdict? verdict = null)
        {
            int cid = Contest.Id;
            return Polygon.Submissions.ListWithJudgingAsync(
                pagination: (page, perPage),
                predicate: Expr.Of<Submission, Judging>((s, j) => s.ContestId == cid)!
                    .CombineIf(probid.HasValue, (s, j) => s.ProblemId == probid)
                    .CombineIf(langid != null, (s, j) => s.Language == langid)
                    .CombineIf(teamid.HasValue, (s, j) => s.TeamId == teamid)
                    .CombineIf(verdict.HasValue, (s, j) => j.Status == verdict));
        }

        public virtual async Task<Solution?> FindSolutionAsync(int submitid)
        {
            int cid = Contest.Id;
            var res = await Polygon.Submissions.ListWithJudgingAsync(s => s.ContestId == cid && s.Id == submitid, true, 1);
            return res.FirstOrDefault();
        }

        public virtual Task<IPagedList<TSolution>> ListSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            Expression<Func<Submission, bool>> predicate,
            int page, int perpage)
        {
            return Polygon.Submissions.ListWithJudgingAsync((page, perpage), selector, predicate);
        }

        public virtual Task<List<TSolution>> ListSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            int? probid = null,
            string? langid = null,
            int? teamid = null)
        {
            int cid = Contest.Id;
            var cond = Expr
                .Of<Submission>(s => s.ContestId == cid)!
                .CombineIf(probid.HasValue, s => s.ProblemId == probid)
                .CombineIf(teamid.HasValue, s => s.TeamId == teamid)
                .CombineIf(!string.IsNullOrEmpty(langid), s => s.Language == langid);
            return Polygon.Submissions.ListWithJudgingAsync(selector, cond);
        }

        public virtual Task<JudgingRun?> GetDetailAsync(int problemId, int submitId, int judgingId, int runId)
        {
            return Polygon.Judgings.GetDetailAsync(problemId, submitId, judgingId, runId);
        }

        public virtual async Task<TSolution?> FindSolutionAsync<TSolution>(
            int submitid,
            Expression<Func<Submission, Judging, TSolution>> selector) where TSolution : class
        {
            int cid = Contest.Id;
            var res = await Polygon.Submissions.ListWithJudgingAsync(selector, s => s.ContestId == cid && s.Id == submitid, 1);
            return res.FirstOrDefault();
        }

        public virtual Task<Submission> SubmitAsync(
            string code,
            Language language,
            ContestProblem problem,
            Team team,
            IPAddress ipAddr,
            string via,
            string username,
            DateTimeOffset? time)
        {
            return Polygon.Submissions.CreateAsync(
                code: code,
                language: language.Id,
                problemId: problem.ProblemId,
                contestId: Contest.Id,
                teamId: team.TeamId,
                ipAddr: ipAddr,
                via: via,
                username: username,
                time: time,
                fullJudge: Contest.RankingStrategy == CcsDefaults.RuleIOI);
        }

        public Task<IEnumerable<(JudgingRun?, Testcase)>> GetDetailsAsync(int problemId, int judgingId)
        {
            return Polygon.Judgings.GetDetailsAsync(problemId, judgingId);
        }

        public Task<IEnumerable<T>> GetDetailsAsync<T>(
            Expression<Func<Testcase, JudgingRun, T>> selector,
            Expression<Func<Testcase, JudgingRun, bool>>? predicate = null,
            int? limit = null)
        {
            return Polygon.Judgings.GetDetailsAsync(selector, predicate, limit);
        }

        public Task<int> CountJudgingsAsync(Expression<Func<Judging, bool>> predicate)
        {
            return Polygon.Judgings.CountAsync(predicate);
        }

        public Task<Judging?> FindJudgingAsync(int id)
        {
            var cid = Contest.Id;
            return Polygon.Judgings.FindAsync(j => j.Id == id && j.s.ContestId == cid, j => j);
        }

        public Task<List<Judging>> ListJudgingsAsync(Expression<Func<Judging, bool>> predicate, int topCount)
        {
            return Polygon.Judgings.ListAsync(predicate, topCount);
        }

        public Task<SubmissionSource?> GetSourceCodeAsync(Expression<Func<Submission, bool>> predicate)
        {
            var cid = Contest.Id;
            return Db.Submissions
                .Where(s => s.ContestId == cid)
                .Where(predicate)
                .OrderByDescending(s => s.Id)
                .Select(s => new SubmissionSource(s.Id, s.ContestId, s.TeamId, s.ProblemId, s.Language, s.SourceCode))
                .FirstOrDefaultAsync();
        }

        public Task<List<T>> ListSolutionsAsync<T>(
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null)
        {
            int cid = Contest.Id;
            predicate = predicate.Combine(s => s.ContestId == cid);
            return Polygon.Submissions.ListWithJudgingAsync(selector, predicate, limits);
        }

        public Task<List<Judging>> ListJudgingsAsync(Expression<Func<Judging, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return Db.Judgings
                .AsNoTracking()
                .Where(j => j.s.ContestId == cid && !j.s.Ignored)
                .WhereIf(predicate != null, predicate)
                .ToListAsync();
        }

        public Task<List<JudgingRun>> ListJudgingRunsAsync(Expression<Func<JudgingRun, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return Db.JudgingRuns
                .AsNoTracking()
                .Where(jr => jr.j.s.ContestId == cid && !jr.j.s.Ignored)
                .WhereIf(predicate != null, predicate)
                .ToListAsync();
        }
    }
}
