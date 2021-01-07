using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Services;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Contexts.Immediate
{
    public class ImmediateContestContext : IContestContext
    {
        private readonly IServiceProvider _services;
        private readonly Contest _contest;
        private IClarificationStore? _clars;
        private ITeamStore? _teams;
        private IProblemsetStore? _probs;
        private IContestStore? _ctsx;
        private IJudgingStore? _judgings;
        private ISubmissionStore? _submits;
        private IRejudgingStore? _rejudgings;

        public Contest Contest => _contest;

        public IClarificationStore ClarificationStore => _clars ??= GetRequiredService<IClarificationStore>();

        public ITeamStore TeamStore => _teams ??= GetRequiredService<ITeamStore>();

        public IProblemsetStore ProblemsetStore => _probs ??= GetRequiredService<IProblemsetStore>();

        public IContestStore ContestStore => _ctsx ??= GetRequiredService<IContestStore>();

        public ISubmissionStore SubmissionStore => _submits ??= GetRequiredService<ISubmissionStore>();

        public IJudgingStore JudgingStore => _judgings ??= GetRequiredService<IJudgingStore>();

        public IRejudgingStore RejudgingStore => _rejudgings ??= GetRequiredService<IRejudgingStore>();

        public ImmediateContestContext(Contest contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
        }

        public virtual async Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            var store = GetRequiredService<ILanguageStore>();
            IReadOnlyList<Language> langs = await store.ListAsync(true);
            if (!string.IsNullOrEmpty(Contest.Languages))
            {
                var available = Contest.Languages!.AsJson<string[]>() ?? Array.Empty<string>();
                langs = langs.Where(l => available.Contains(l.Id)).ToList();
            }

            return langs;
        }

        public virtual Task<IReadOnlyList<ProblemModel>> FetchProblemsAsync()
        {
            return ProblemsetStore.ListAsync(Contest);
        }

        public virtual Task<ScoreboardModel> FetchScoreboardAsync()
        {
            return TeamStore.LoadScoreboardAsync(Contest);
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
            return SubmissionStore.CreateAsync(
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

        public virtual Task<Team?> FindTeamByIdAsync(int teamId)
        {
            return TeamStore.FindByIdAsync(Contest.Id, teamId);
        }

        public virtual Task<Member?> FindMemberByUserAsync(int userId)
        {
            return TeamStore.FindByUserAsync(Contest.Id, userId);
        }

        public virtual async Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression)
        {
            await ContestStore.UpdateAsync(Contest.Id, updateExpression);
            return await ContestStore.FindAsync(Contest.Id);
        }

        public T GetRequiredService<T>()
        {
            return _services.GetRequiredService<T>();
        }

        public virtual Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool contestFiltered)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool contestFiltered)
        {
            throw new NotImplementedException();
        }

        public virtual Task UpdateProblemAsync(ProblemModel origin, Expression<Func<ContestProblem>> expression)
        {
            return ProblemsetStore.UpdateAsync(origin.ContestId, origin.ProblemId, expression);
        }

        public virtual Task<HashSet<int>> FetchJuryAsync()
        {
            return ContestStore.ListJuryAsync(Contest);
        }

        public virtual Task AssignJuryAsync(IUser user)
        {
            return ContestStore.AssignJuryAsync(Contest, user);
        }

        public virtual Task UnassignJuryAsync(IUser user)
        {
            return ContestStore.UnassignJuryAsync(Contest, user);
        }

        public virtual Task UpdateTeamAsync(Team origin, Expression<Func<Team>> expression)
        {
            return TeamStore.UpdateAsync(origin.ContestId, origin.TeamId, expression);
        }

        public virtual Task CreateProblemAsync(Expression<Func<ContestProblem>> expression)
        {
            return ProblemsetStore.CreateAsync(expression.Compile().Invoke());
        }

        public virtual Task DeleteProblemAsync(ProblemModel problem)
        {
            return ProblemsetStore.DeleteAsync(problem);
        }

        public virtual async Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync()
        {
            var list = await TeamStore.ListAsync(t => new { t.TeamId, t.TeamName }, t => t.Status == 1);
            return list.ToDictionary(k => k.TeamId, k => k.TeamName);
        }

        public virtual async Task<IReadOnlyDictionary<int, (string Name, string Affiliation)>> FetchPublicTeamNamesWithAffiliationAsync()
        {
            var list = await TeamStore.ListAsync(t => new { t.TeamId, t.TeamName, t.Affiliation.Abbreviation }, t => t.Status == 1 && t.Category.IsPublic);
            return list.ToDictionary(k => k.TeamId, k => (k.TeamName, k.Abbreviation));
        }

        public virtual Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin)
        {
            return TeamStore.DeleteAsync(origin);
        }

        public virtual Task<ILookup<int, string>> FetchTeamMembersAsync()
        {
            return TeamStore.ListMembersAsync(Contest);
        }

        public virtual Task<IEnumerable<string>> FetchTeamMemberAsync(Team team)
        {
            return TeamStore.ListMembersAsync(team);
        }

        public virtual Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
        {
            return TeamStore.CreateAsync(team, users);
        }

        public Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate)
        {
            return ClarificationStore.ListAsync(Contest, predicate);
        }

        public Task<Clarification> FindClarificationAsync(int id)
        {
            return ClarificationStore.FindAsync(Contest, id);
        }

        public Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null)
        {
            return ClarificationStore.SendAsync(clar, replyTo);
        }

        public virtual Task<string> GetReadmeAsync(bool source)
        {
            throw new NotImplementedException();
        }

        public async Task<IPagedList<Auditlog>> ViewLogsAsync(int page, int pageCount)
        {
            return await GetRequiredService<IAuditlogger>().ViewLogsAsync(Contest.Id, page, pageCount);
        }

        public virtual async Task<object> GetUpdatesAsync()
        {
            return new
            {
                clarifications = await ClarificationStore.CountUnansweredAsync(Contest),
                teams = await TeamStore.CountPendingAsync(Contest),
                rejudgings = await RejudgingStore.CountUndoneAsync(Contest.Id),
            };
        }

        public virtual Task SetReadmeAsync(string source)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> FetchEventAsync(string[]? type, int after)
        {
            return ContestStore.FetchEventAsync(Contest.Id, type, after);
        }

        public Task<int> MaxEventIdAsync()
        {
            return ContestStore.MaxEventIdAsync(Contest.Id);
        }

        public Task<List<T>> ListTeamsAsync<T>(Expression<Func<Team, T>> selector, Expression<Func<Team, bool>>? predicate = null) where T : class
        {
            int cid = Contest.Id;
            return TeamStore.ListAsync(selector, predicate.Combine(t => t.ContestId == cid));
        }

        public async Task<ServerStatus> GetJudgeQueueAsync()
        {
            var lists = await JudgingStore.GetJudgeQueueAsync(Contest.Id);
            return lists.SingleOrDefault() ?? new ServerStatus { ContestId = Contest.Id };
        }

        public async Task<Submission?> FindSubmissionAsync(int submissionId, bool includeJudgings = false)
        {
            var result = await SubmissionStore.FindAsync(submissionId, includeJudgings);
            return result?.ContestId == Contest.Id ? result : null;
        }

        public Task<List<T>> ListSubmissionsAsync<T>(Expression<Func<Submission, T>> projection, Expression<Func<Submission, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return SubmissionStore.ListAsync(projection, predicate.Combine(s => s.ContestId == cid));
        }

        public Task<List<Solution>> FetchSolutionsAsync(int? probid = null, string? langid = null, int? teamid = null, bool all = false)
        {
            int cid = Contest.Id;
            Expression<Func<Submission, bool>> cond = s => s.ContestId == cid;
            if (probid.HasValue) cond = cond.Combine(s => s.ProblemId == probid);
            if (teamid.HasValue) cond = cond.Combine(s => s.TeamId == teamid);
            if (!string.IsNullOrEmpty(langid)) cond = cond.Combine(s => s.Language == langid);
            int? limit = all ? default(int?) : 75;

            return SubmissionStore.ListWithJudgingAsync(cond, true, limit);
        }

        public Task<IPagedList<Solution>> FetchSolutionsAsync(int page, int perPage)
        {
            int cid = Contest.Id;
            return SubmissionStore.ListWithJudgingAsync((page, perPage), s => s.ContestId == cid);
        }

        public async Task<Solution> FetchSolutionAsync(int submitid)
        {
            int cid = Contest.Id;
            var res = await SubmissionStore.ListWithJudgingAsync(s => s.ContestId == cid && s.Id == submitid, true, 1);
            return res.FirstOrDefault();
        }

        public Task<List<TSolution>> FetchSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            int? probid = null,
            string? langid = null,
            int? teamid = null)
        {
            int cid = Contest.Id;
            Expression<Func<Submission, bool>> cond = s => s.ContestId == cid;
            if (probid.HasValue) cond = cond.Combine(s => s.ProblemId == probid);
            if (teamid.HasValue) cond = cond.Combine(s => s.TeamId == teamid);
            if (!string.IsNullOrEmpty(langid)) cond = cond.Combine(s => s.Language == langid);
            return SubmissionStore.ListWithJudgingAsync(selector, cond);
        }

        public async Task<TSolution> FetchSolutionAsync<TSolution>(int submitid, Expression<Func<Submission, Judging, TSolution>> selector)
        {
            int cid = Contest.Id;
            var res = await SubmissionStore.ListWithJudgingAsync(selector, s => s.ContestId == cid && s.Id == submitid, 1);
            return res.FirstOrDefault();
        }

        public Task<Affiliation?> FetchAffiliationAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BalloonModel>> FetchBalloonsAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetBalloonDoneAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetClarificationAnsweredAsync(int id, bool answered)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ClaimClarificationAsync(int id, string jury, bool claim)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Available, string Message)> CheckProblemAvailabilityAsync(int probId, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Statement>> FetchRawStatementsAsync()
        {
            var store = GetRequiredService<IProblemsetStore>();
            var problems = await FetchProblemsAsync();
            var provider = GetRequiredService<Polygon.Packaging.IStatementProvider>();
            var raw = await store.RawProblemsAsync(Contest.Id);
            var stmts = new List<Statement>();
            foreach (var prob in raw)
            {
                var stmt = await provider.ReadAsync(prob);
                stmts.Add(new Statement(prob,
                    stmt.Description, stmt.Input, stmt.Output, stmt.Hint, stmt.Interaction,
                    problems.FirstOrDefault(p => p.ProblemId == prob.Id).ShortName, stmt.Samples));
            }

            return stmts;
        }

        public Task<IEnumerable<(JudgingRun, Testcase)>> FetchDetailsAsync(int problemId, int judgingId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FetchDetailsAsync<T>(Expression<Func<Testcase, JudgingRun, T>> selector, Expression<Func<Testcase, JudgingRun, bool>>? predicate = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public Task<Rejudging> CreateRejudgingAsync(Rejudging entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRejudgingAsync(Rejudging entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRejudgingAsync(int id, Expression<Func<Rejudging, Rejudging>> expression)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRejudgingAsync(Rejudging entity)
        {
            throw new NotImplementedException();
        }

        public Task<Rejudging> FindRejudgingAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Rejudging>> FetchRejudgingsAsync(bool includeStat = true)
        {
            throw new NotImplementedException();
        }

        public Task<List<Judgehost>> FetchJudgehostsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RejudgingDifference>> ViewRejudgingAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> BatchRejudgeAsync(Expression<Func<Submission, Judging, bool>> predicate, Rejudging? rejudge = null, bool fullTest = false)
        {
            throw new NotImplementedException();
        }

        public Task CancelRejudgingAsync(Rejudging rejudge, int uid)
        {
            throw new NotImplementedException();
        }

        public Task ApplyRejudgingAsync(Rejudging rejudge, int uid)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountJudgingAsync(Expression<Func<Judging, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<Judging> FindJudgingAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Judging>> FetchJudgingsAsync(Expression<Func<Judging, bool>> predicate, int topCount)
        {
            throw new NotImplementedException();
        }

        public Task<SubmissionSource> FetchSourceAsync(Expression<Func<Submission, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
