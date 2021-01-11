using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Team"/> and <see cref="Member"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface ITeamStore
    {
        /*

        [Obsolete]
        Task<List<T>> ListAsync<T>(int cid,
            Expression<Func<Team, T>> selector,
            Expression<Func<Team, bool>>? predicate = null,
            (string, TimeSpan)? cacheTag = null);

        [Obsolete]
        Task<T> FindAsync<T>(int cid, int tid,
            Expression<Func<Team, T>> selector);

        [Obsolete]
        Task<HashSet<int>> ListRegisteredAsync(int uid);

        [Obsolete]
        Task<List<Member>> ListRegisteredWithDetailAsync(int uid);

        [Obsolete]
        Task<HashSet<int>> ListMemberUidsAsync(int cid);

        [Obsolete]
        Task<Dictionary<int, string>> ListNamesAsync(int cid);

        [Obsolete]
        Task<Dictionary<int, (int ac, int tot)>> StatisticsSubmissionAsync(int cid, int teamid);

        */
    }
}
