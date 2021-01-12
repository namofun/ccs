using Ccs.Entities;
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

        public override async Task<Contest> UpdateContestAsync(
            Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            Expire("Core");
            Expire("Languages");

            // The other occurrence is in Factory.cs
            return await CacheAsync("Core", _options.Contest,
                async () => await Ccs.FindAsync(Contest.Id));
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

        public override Task<HashSet<int>> FetchJuryAsync()
        {
            return CacheAsync("Jury", _options.Contest,
                async () => await base.FetchJuryAsync());
        }
    }
}
