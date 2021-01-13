﻿using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual async Task<ServerStatus> GetJudgeQueueAsync()
        {
            var lists = await Polygon.Judgings.GetJudgeQueueAsync(Contest.Id);
            return lists.SingleOrDefault() ?? new ServerStatus { ContestId = Contest.Id };
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

        public virtual Task<List<Solution>> FetchSolutionsAsync(int? probid = null, string? langid = null, int? teamid = null, bool all = false)
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

        public virtual Task<IPagedList<Solution>> FetchSolutionsAsync(int page, int perPage)
        {
            int cid = Contest.Id;
            return Polygon.Submissions.ListWithJudgingAsync((page, perPage), s => s.ContestId == cid);
        }

        public virtual async Task<Solution> FetchSolutionAsync(int submitid)
        {
            int cid = Contest.Id;
            var res = await Polygon.Submissions.ListWithJudgingAsync(s => s.ContestId == cid && s.Id == submitid, true, 1);
            return res.FirstOrDefault();
        }

        public virtual Task<List<TSolution>> FetchSolutionsAsync<TSolution>(
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

        public virtual async Task<TSolution> FetchSolutionAsync<TSolution>(
            int submitid,
            Expression<Func<Submission, Judging, TSolution>> selector)
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
                fullJudge: Contest.RankingStrategy == 1);
        }

        public Task<IEnumerable<(JudgingRun?, Testcase)>> FetchDetailsAsync(int problemId, int judgingId)
        {
            return Polygon.Judgings.GetDetailsAsync(problemId, judgingId);
        }

        public Task<IEnumerable<T>> FetchDetailsAsync<T>(
            Expression<Func<Testcase, JudgingRun, T>> selector,
            Expression<Func<Testcase, JudgingRun, bool>>? predicate = null,
            int? limit = null)
        {
            return Polygon.Judgings.GetDetailsAsync(selector, predicate, limit);
        }

        public Task<int> CountJudgingAsync(Expression<Func<Judging, bool>> predicate)
        {
            return Polygon.Judgings.CountAsync(predicate);
        }

        public Task<Judging> FindJudgingAsync(int id)
        {
            var cid = Contest.Id;
            return Polygon.Judgings.FindAsync(j => j.Id == id && j.s.ContestId == cid, j => j);
        }

        public Task<List<Judging>> FetchJudgingsAsync(Expression<Func<Judging, bool>> predicate, int topCount)
        {
            return Polygon.Judgings.ListAsync(predicate, topCount);
        }

        public Task<SubmissionSource> FetchSourceAsync(Expression<Func<Submission, bool>> predicate)
        {
            var cid = Contest.Id;
            return Db.Submissions
                .Where(s => s.ContestId == cid)
                .Where(predicate)
                .OrderByDescending(s => s.Id)
                .Select(s => new SubmissionSource(s.Id, s.ContestId, s.TeamId, s.ProblemId, s.Language, s.SourceCode))
                .FirstOrDefaultAsync();
        }

        public Task<List<T>> FetchSolutionsAsync<T>(
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null)
        {
            return Polygon.Submissions.ListWithJudgingAsync(selector, predicate, limits);
        }
    }
}