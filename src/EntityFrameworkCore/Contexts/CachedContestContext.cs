using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Contexts.Cached
{
    public class CachedContestContext : Immediate.ImmediateContestContext
    {
        private readonly IMemoryCache _cache;
        private readonly CachedContestContextIntervalOptions _options;

        public CachedContestContext(
            Contest contest,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            IOptions<CachedContestContextIntervalOptions> options) :
            base(contest, serviceProvider)
        {
            _cache = cache;
            _options = options.Value;
        }

        private Task<TValue> CacheAsync<TValue>(string t, TimeSpan? s, Func<Task<TValue>> f)
        {
            return _cache.GetOrCreateAsync($"Context({Contest.Id})::{t}", async entry =>
            {
                var value = await f();
                entry.AbsoluteExpirationRelativeToNow = s;
                return value;
            });
        }

        private void Expire(string tag)
        {
            _cache.Remove($"Context({Contest.Id})::{tag}");
        }

        #region Aggregate Root: Jury

        public override async Task UnassignJuryAsync(IUser user)
        {
            await base.UnassignJuryAsync(user);
            Expire("Jury");
        }

        public override async Task AssignJuryAsync(IUser user)
        {
            await base.AssignJuryAsync(user);
            Expire("Jury");
        }

        public override Task<HashSet<int>> FetchJuryAsync()
        {
            return CacheAsync("Jury", _options.Contest,
                async () => await base.FetchJuryAsync());
        }

        #endregion

        #region Aggregate Root: Contest

        public override Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            return CacheAsync("Languages", _options.Language,
                async () => await base.FetchLanguagesAsync());
        }

        public override async Task<Contest> UpdateContestAsync(
            Expression<Func<Contest, Contest>> updateExpression)
        {
            await ContestStore.UpdateAsync(Contest.Id, updateExpression);
            Expire("Core");
            Expire("Languages");

            // The other occurrence is in Factory.cs
            return await CacheAsync("Core", _options.Contest,
                async () => await ContestStore.FindAsync(Contest.Id));
        }

        public override Task<object> GetUpdatesAsync()
        {
            return CacheAsync("Updates", TimeSpan.FromSeconds(10),
                async () => await base.GetUpdatesAsync());
        }

        #endregion

        #region Aggregate Root: Problem

        public override Task<IReadOnlyList<ProblemModel>> FetchProblemsAsync()
        {
            return CacheAsync("Problems", _options.Problem,
                async () => await base.FetchProblemsAsync());
        }

        public override async Task UpdateProblemAsync(
            ProblemModel origin,
            Expression<Func<ContestProblem>> expression)
        {
            await base.UpdateProblemAsync(origin, expression);
            Expire("Problems");
        }

        public override async Task CreateProblemAsync(Expression<Func<ContestProblem>> expression)
        {
            await base.CreateProblemAsync(expression);
            Expire("Problems");
        }

        public override async Task DeleteProblemAsync(ProblemModel problem)
        {
            await base.DeleteProblemAsync(problem);
            Expire("Problems");
        }

        #endregion

        #region Aggregate Root: Team

        private void ExpireTeamThings(string one, IEnumerable<string>? another = null)
        {
            Expire(one);
            foreach (var item in another ?? Array.Empty<string>())
                Expire(item);
            Expire("Teams::Affiliations(Filtered=True)");
            Expire("Teams::Categories(Filtered=True)");
            Expire("Teams::Names");
            Expire("Teams::Analysis");
            Expire("Teams::Members");
        }

        public override async Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin)
        {
            var members = await base.DeleteTeamAsync(origin);

            ExpireTeamThings(
                $"Teams::Id({origin.TeamId})",
                members.Select(m => $"Teams::User({m.UserId})"));

            return members;
        }

        public override Task<Team?> FindTeamByIdAsync(int teamId)
        {
            return CacheAsync($"Teams::Id({teamId})", _options.Team,
                async () => await base.FindTeamByIdAsync(teamId));
        }

        public override Task<Member?> FindMemberByUserAsync(int userId)
        {
            return CacheAsync($"Teams::User({userId})", _options.Team,
                async () => await base.FindMemberByUserAsync(userId));
        }

        public override Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool filtered)
        {
            return CacheAsync($"Teams::Affiliations(Filtered={filtered})", _options.Teams,
                async () => await base.FetchAffiliationsAsync(filtered));
        }

        public override Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool filtered)
        {
            return CacheAsync($"Teams::Categories(Filtered={filtered})", _options.Teams,
                async () => await base.FetchCategoriesAsync(filtered));
        }

        public override async Task UpdateTeamAsync(Team origin, Expression<Func<Team>> expression)
        {
            await base.UpdateTeamAsync(origin, expression);
            ExpireTeamThings($"Teams::Id({origin.TeamId})");
        }

        public override Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync()
        {
            return CacheAsync("Teams::Names", _options.Teams,
                async () => await base.FetchTeamNamesAsync());
        }

        public override Task<IReadOnlyDictionary<int, (string Name, string Affiliation)>> FetchPublicTeamNamesWithAffiliationAsync()
        {
            return CacheAsync("Teams::Analysis", TimeSpan.FromMinutes(2),
                async () => await base.FetchPublicTeamNamesWithAffiliationAsync());
        }

        public override async Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
        {
            team = await base.CreateTeamAsync(team, users);

            ExpireTeamThings(
                $"Teams::Id({team.TeamId})",
                users?.Select(m => $"Teams::User({m.Id})"));

            return team;
        }

        public override Task<ILookup<int, string>> FetchTeamMembersAsync()
        {
            return CacheAsync("Teams::Members", _options.Teams,
                async () => await base.FetchTeamMembersAsync());
        }

        public override async Task<IEnumerable<string>> FetchTeamMemberAsync(Team team)
        {
            return (await FetchTeamMembersAsync())[team.TeamId];
        }

        #endregion
    }
}
