using Ccs.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The special used contest repository to list the contests for users.
    /// </summary>
    public interface IContestRepository2
    {
        /// <summary>
        /// Lists the contests with user and kind limitation.
        /// </summary>
        /// <param name="user">The user claims principal.</param>
        /// <param name="kind">The contest kind.</param>
        /// <param name="page">The current page.</param>
        /// <param name="limit">The count per page.</param>
        /// <returns>The task for fetching paged lists.</returns>
        Task<IPagedList<ContestListModel>> ListAsync(ClaimsPrincipal user, int? kind = null, int page = 1, int limit = 100);
    }
}
