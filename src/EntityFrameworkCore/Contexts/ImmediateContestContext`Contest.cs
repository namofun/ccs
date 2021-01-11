using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual async Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            IReadOnlyList<Language> langs = await Polygon.Languages.ListAsync(true);
            if (!string.IsNullOrEmpty(Contest.Languages))
            {
                var available = Contest.Languages!.AsJson<string[]>() ?? Array.Empty<string>();
                langs = langs.Where(l => available.Contains(l.Id)).ToList();
            }

            return langs;
        }

        public virtual async Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.ContestStore.UpdateAsync(Contest.Id, updateExpression);
            return await Ccs.ContestStore.FindAsync(Contest.Id);
        }

        public virtual Task<HashSet<int>> FetchJuryAsync()
        {
            return Ccs.ContestStore.ListJuryAsync(Contest.Id);
        }

        public virtual Task AssignJuryAsync(IUser user)
        {
            return Ccs.ContestStore.AssignJuryAsync(Contest.Id, user.Id);
        }

        public virtual Task UnassignJuryAsync(IUser user)
        {
            return Ccs.ContestStore.UnassignJuryAsync(Contest.Id, user.Id);
        }

        public virtual Task<List<Event>> FetchEventAsync(string[]? type, int after)
        {
            return Ccs.ContestStore.FetchEventAsync(Contest.Id, type, after);
        }

        public virtual Task<int> MaxEventIdAsync()
        {
            return Ccs.ContestStore.MaxEventIdAsync(Contest.Id);
        }

        public async Task<IPagedList<Auditlog>> ViewLogsAsync(int page, int pageCount)
        {
            return await _services.GetRequiredService<IAuditlogger>().ViewLogsAsync(Contest.Id, page, pageCount);
        }

        public virtual Task<string> GetReadmeAsync(bool source)
        {
            throw new NotImplementedException();
        }

        public virtual Task SetReadmeAsync(string source)
        {
            throw new NotImplementedException();
        }
    }
}
