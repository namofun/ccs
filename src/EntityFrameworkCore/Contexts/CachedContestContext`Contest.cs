using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CachedContestContext
    {
        public override Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            return CacheAsync("Languages", _options.Language,
                async () => await base.FetchLanguagesAsync());
        }

        public override async Task<ContestWrapper> UpdateContestAsync(
            Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            Expire("Core");
            Expire("Languages");

            // The other occurrence is in Factory.cs
            return await CacheAsync("Core", _options.Contest,
                async () => (await Ccs.FindAsync(Contest.Id))!);
        }

        public override Task<object> GetUpdatesAsync()
        {
            return CacheAsync("Updates", TimeSpan.FromSeconds(10),
                async () => await base.GetUpdatesAsync());
        }

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

        public override Task<Dictionary<int, string>> FetchJuryAsync()
        {
            return CacheAsync("Jury", _options.Contest,
                async () => await base.FetchJuryAsync());
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
    }
}
