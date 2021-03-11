using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CachedContestContext
    {
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
            if (filtered)
            {
                var langs = await ListLanguagesAsync();
                return langs.FirstOrDefault(l => l.Id == langid);
            }
            else
            {
                return await base.FindLanguageAsync(langid, false);
            }
        }

        public override async Task<ContestWrapper> UpdateContestAsync(
            Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            Expire("Core");
            Expire("Languages");

            // The other occurrence is in Factory.cs
            var @new = await CacheAsync("Core", _options.Contest,
                async () => (await Ccs.FindAsync(Contest.Id))!);

            await Mediator.Publish(new Events.ContestUpdateEvent(Contest, @new));
            return @new;
        }

        public override Task<Dictionary<int, string>> ListJuriesAsync()
        {
            return CacheAsync("Jury", _options.Contest,
                async () => await base.ListJuriesAsync());
        }

        public override async Task AssignJuryAsync(IUser user)
        {
            await base.AssignJuryAsync(user);
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
