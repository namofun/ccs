using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// Adds the rating claim into the user information.
    /// </summary>
    public class RatingClaimsProvider : IUserClaimsProvider
    {
        /// <summary>
        /// The claims name of rating
        /// </summary>
        public const string RatingClaimsName = "rating";

        /// <inheritdoc />
        public Task<IEnumerable<Claim>> GetClaimsAsync(IUser user)
        {
            var rating = ((IUserWithRating)user).Rating;
            return Task.FromResult(rating.HasValue
                ? new[] { new Claim(RatingClaimsName, rating.Value.ToString()) }
                : Enumerable.Empty<Claim>());
        }
    }
}
