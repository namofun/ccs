using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    public partial class CachedContestContext
    {
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

        public override async Task<Affiliation?> FindAffiliationAsync(int id, bool filtered = true)
        {
            var results = await ListAffiliationsAsync(filtered);
            return results.GetValueOrDefault(id);
        }

        public override async Task<Affiliation?> FindAffiliationAsync(string id, bool filtered = true)
        {
            var results = await ListAffiliationsAsync(filtered);
            return results.Values.FirstOrDefault(a => a.Abbreviation == id);
        }

        public override async Task<Category?> FindCategoryAsync(int id, bool filtered = true)
        {
            var results = await ListCategoriesAsync(filtered);
            return results.GetValueOrDefault(id);
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

        public override Task<IReadOnlyDictionary<int, Affiliation>> ListAffiliationsAsync(bool filtered)
        {
            return CacheAsync($"Teams::Affiliations(Filtered={filtered})", _options.Teams,
                async () => await base.ListAffiliationsAsync(filtered));
        }

        public override Task<IReadOnlyDictionary<int, Category>> ListCategoriesAsync(bool filtered)
        {
            return CacheAsync($"Teams::Categories(Filtered={filtered})", _options.Teams,
                async () => await base.ListCategoriesAsync(filtered));
        }

        public override async Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
        {
            team = await base.CreateTeamAsync(team, users);

            ExpireTeamThings(
                $"Teams::Id({team.TeamId})",
                users?.Select(m => $"Teams::User({m.Id})"));

            return team;
        }

        public override async Task UpdateTeamAsync(Team origin, Expression<Func<Team, Team>> expression)
        {
            await base.UpdateTeamAsync(origin, expression);
            ExpireTeamThings($"Teams::Id({origin.TeamId})");
        }

        public override async Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin)
        {
            var members = await base.DeleteTeamAsync(origin);

            ExpireTeamThings(
                $"Teams::Id({origin.TeamId})",
                members.Select(m => $"Teams::User({m.UserId})"));

            return members;
        }

        public override Task<IReadOnlyDictionary<int, string>> GetTeamNamesAsync()
        {
            return CacheAsync("Teams::Names", _options.Teams,
                async () => await base.GetTeamNamesAsync());
        }

        public override Task<ILookup<int, string>> GetTeamMembersAsync()
        {
            return CacheAsync("Teams::Members", _options.Teams,
                async () => await base.GetTeamMembersAsync());
        }

        public override async Task<IEnumerable<string>> GetTeamMemberAsync(Team team)
        {
            return (await GetTeamMembersAsync())[team.TeamId];
        }

        public override Task<IReadOnlyDictionary<int, AnalyticalTeam>> GetAnalyticalTeamsAsync()
        {
            return CacheAsync("Teams::Analysis", TimeSpan.FromMinutes(2),
                async () => await base.GetAnalyticalTeamsAsync());
        }

        public override Task<ScoreboardModel> GetScoreboardAsync()
        {
            return CacheAsync("Teams::Scoreboard", _options.Scoreboard,
                async () => await base.GetScoreboardAsync());
        }

        public override Task<IReadOnlyDictionary<int, (int, int)>> StatisticsAsync(Team? team)
        {
            if (team == null) return Task.FromResult(_emptyStat);
            return CacheAsync($"Teams::Statistics({team.TeamId})", _options.Statistics,
                async () => await base.StatisticsAsync(team));
        }

        public override Task<IReadOnlyDictionary<int, (int, int, int, int)>> StatisticsGlobalAsync()
        {
            return CacheAsync($"Teams::Statistics(*)", _options.Statistics,
                async () => await base.StatisticsGlobalAsync());
        }

        public override async Task AttachMemberAsync(Team team, IUser user, bool temporary)
        {
            await base.AttachMemberAsync(team, user, temporary);
            Expire($"Teams::User({user.Id})");
        }

        public override async Task<List<Member>> LockOutTemporaryAsync(IUserManager userManager)
        {
            var result = await base.LockOutTemporaryAsync(userManager);

            ExpireTeamThings(
                "Teams::Members",
                result.Select(m => $"Teams::User({m.UserId})"));

            return result;
        }

        public override Task<HashSet<int>> GetVisibleTenantsAsync()
        {
            return CacheAsync("Tenants", _options.Contest,
                async () => await base.GetVisibleTenantsAsync());
        }

        public override async Task AllowTenantAsync(Affiliation affiliation)
        {
            await base.AllowTenantAsync(affiliation);
            Expire("Tenants");
        }

        public override async Task DisallowTenantAsync(Affiliation affiliation)
        {
            await base.DisallowTenantAsync(affiliation);
            Expire("Tenants");
        }

        public override async Task<bool> IsTenantVisibleAsync(IEnumerable<int> tenants)
        {
            var allowed = await GetVisibleTenantsAsync();
            return tenants.Any(a => allowed.Contains(a));
        }
    }
}
