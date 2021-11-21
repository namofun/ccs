using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CachedContestContext
    {
        public override Task<IReadOnlyList<(IPAddress Address, int Subnet)>?> ListIpRangesAsync()
        {
            return CacheAsync("IpRanges", _options.Language,
                async () => await base.ListIpRangesAsync());
        }

        public override Task<IReadOnlyList<Language>> ListLanguagesAsync(bool filtered = true)
        {
            if (filtered)
            {
                return CacheAsync("Languages", _options.Language,
                    async () => await base.ListLanguagesAsync(true));
            }
            else
            {
                return base.ListLanguagesAsync(false);
            }
        }

        public override async Task<Language?> FindLanguageAsync(string? langid, bool filtered = true)
        {
            var langs = await ListLanguagesAsync();
            var lang = langs.FirstOrDefault(l => l.Id == langid);
            if (lang == null && !filtered)
            {
                lang = await base.FindLanguageAsync(langid, filtered: false);
            }

            return lang;
        }

        public override async Task<ContestWrapper> UpdateContestAsync(
            Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            Expire("Core");
            Expire("Languages");
            Expire("IpRanges");
            Expire("State");

            // The other occurrence is in Factory.cs
            var @new = await CacheAsync("Core", _options.Contest,
                async () => (await Ccs.FindAsync(Contest.Id))!);

            await Mediator.Publish(new Events.ContestUpdateEvent(Contest, @new, this));
            return @new;
        }

        public override Task<Dictionary<int, (string, JuryLevel)>> ListJuriesAsync()
        {
            return CacheAsync("Jury", _options.Contest,
                async () => await base.ListJuriesAsync());
        }

        public override async Task AssignJuryAsync(IUser user, JuryLevel level)
        {
            await base.AssignJuryAsync(user, level);
            Expire("Jury");
        }

        public override async Task UnassignJuryAsync(IUser user)
        {
            await base.UnassignJuryAsync(user);
            Expire("Jury");
        }

        public override Task<string> GetReadmeAsync(bool source)
        {
            if (source) return base.GetReadmeAsync(true);
            return CacheAsync("Readme", _options.Contest,
                async () => await base.GetReadmeAsync(false));
        }

        public override async Task SetReadmeAsync(string source)
        {
            await base.SetReadmeAsync(source);
            Expire("Readme");
        }

        public override Task<IReadOnlyDictionary<string, object>> GetUpdatesAsync()
        {
            return CacheAsync("Updates", TimeSpan.FromSeconds(10),
                async () => await base.GetUpdatesAsync());
        }
    }
}
