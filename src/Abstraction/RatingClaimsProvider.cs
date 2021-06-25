using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Adds the rating claim into the user information.
    /// </summary>
    public class RatingClaimsProvider : IUserClaimsProvider
    {
        /// <inheritdoc />
        public Task<IEnumerable<Claim>> GetClaimsAsync(IUser user)
        {
            var rating = ((IUserWithRating)user).Rating;
            return Task.FromResult(rating.HasValue
                ? new[] { new Claim("rating", rating.Value.ToString()) }
                : Enumerable.Empty<Claim>());
        }
    }
}
