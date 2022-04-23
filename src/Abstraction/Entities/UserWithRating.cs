using Microsoft.AspNetCore.Identity;

namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// Represents a user with rating in the identity system.
    /// </summary>
    public interface IUserWithRating : IUser
    {
        /// <summary>
        /// Gets or sets the rating of this user.
        /// </summary>
        /// <value>The user rating, null if unrated.</value>
        public int? Rating { get; set; }
    }
}
