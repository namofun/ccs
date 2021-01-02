using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Services;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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

        public Contest Contest => _contest;

        public IClarificationStore ClarificationStore => _clars ??= GetRequiredService<IClarificationStore>();

        public ITeamStore TeamStore => _teams ??= GetRequiredService<ITeamStore>();

        public IProblemsetStore ProblemsetStore => _probs ??= GetRequiredService<IProblemsetStore>();

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
            return GetRequiredService<ISubmissionStore>().CreateAsync(
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
            var store = GetRequiredService<IContestStore>();
            await store.UpdateAsync(Contest.Id, updateExpression);
            return await store.FindAsync(Contest.Id);
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
            return GetRequiredService<IContestStore>().ListJuryAsync(Contest);
        }

        public virtual Task AssignJuryAsync(IUser user)
        {
            return GetRequiredService<IContestStore>()
                .AssignJuryAsync(Contest, user);
        }

        public virtual Task UnassignJuryAsync(IUser user)
        {
            return GetRequiredService<IContestStore>()
                .UnassignJuryAsync(Contest, user);
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

        public Task<string> GetReadmeAsync()
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
                rejudgings = await GetRequiredService<IRejudgingStore>().CountUndoneAsync(Contest.Id),
            };
        }

        public Task SetReadmeAsync(string content)
        {
            throw new NotImplementedException();
        }
    }
}
