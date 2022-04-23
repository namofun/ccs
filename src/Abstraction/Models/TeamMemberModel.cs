using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xylab.Contesting.Models
{
    /// <summary>
    /// The model class for team members.
    /// </summary>
    public class TeamMemberModel : IReadOnlyDictionary<string, string>
    {
        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// The user ID
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// The user name
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The last login IP for user
        /// </summary>
        public string? LastLoginIp { get; }

        /// <summary>
        /// The last rating of participant
        /// </summary>
        public int? PreviousRating { get; }

        /// <summary>
        /// Whether rating is enabled
        /// </summary>
        public bool HasRating { get; }

        /// <summary>
        /// Instantiate the <see cref="TeamMemberModel"/>.
        /// </summary>
        public TeamMemberModel(int teamid, int userId, string userName, string? lastLoginIp)
        {
            TeamId = teamid;
            UserId = userId;
            UserName = userName;
            LastLoginIp = lastLoginIp;
            HasRating = false;
            PreviousRating = null;
        }

        /// <summary>
        /// Instantiate the <see cref="TeamMemberModel"/> with rating support.
        /// </summary>
        public TeamMemberModel(int teamid, int userId, string userName, int? rating, string? lastLoginIp)
        {
            TeamId = teamid;
            UserId = userId;
            UserName = userName;
            LastLoginIp = lastLoginIp;
            HasRating = true;
            PreviousRating = rating;
        }

        /// <inheritdoc />
        public override string ToString()
            => UserName;

        /// <summary>Gets the rating field.</summary>
        private const string ExplicitRating = "explicit-rating";

        /// <summary>Gets the rating description.</summary>
        private string GetRatingDescription()
            => HasRating
                ? PreviousRating?.ToString() ?? "none"
                : throw new InvalidOperationException("There's no rating support.");

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys
            => HasRating
                ? Array.Empty<string>()
                : Enumerable.Repeat(ExplicitRating, 1);

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, string>.Values
            => HasRating
                ? Array.Empty<string>()
                : Enumerable.Repeat(GetRatingDescription(), 1);

        /// <inheritdoc />
        int IReadOnlyCollection<KeyValuePair<string, string>>.Count
            => HasRating ? 1 : 0;

        /// <inheritdoc />
        string IReadOnlyDictionary<string, string>.this[string key]
            => HasRating && key == ExplicitRating
                ? GetRatingDescription()
                : null!;

        /// <inheritdoc />
        bool IReadOnlyDictionary<string, string>.ContainsKey(string key)
            => HasRating && key == ExplicitRating;

        /// <inheritdoc />
        bool IReadOnlyDictionary<string, string>.TryGetValue(string key, out string value)
        {
            value = HasRating && key == ExplicitRating ? GetRatingDescription() : null!;
            return HasRating && key == ExplicitRating;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            if (HasRating) yield return new KeyValuePair<string, string>();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();
        }

        /// <inheritdoc />
        public static implicit operator string(TeamMemberModel a)
        {
            return a.UserName;
        }
    }
}
