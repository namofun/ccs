using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Storages;
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

        public Contest Contest => _contest;

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
            return GetRequiredService<IProblemsetStore>()
                .ListAsync(Contest);
        }

        public virtual Task<ScoreboardModel> FetchScoreboardAsync()
        {
            return GetRequiredService<ITeamStore>()
                .LoadScoreboardAsync(Contest);
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
            return GetRequiredService<ITeamStore>()
                .FindByIdAsync(Contest.Id, teamId);
        }

        public virtual Task<Team?> FindTeamByUserAsync(int userId)
        {
            return GetRequiredService<ITeamStore>()
                .FindByUserAsync(Contest.Id, userId);
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

        public Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
