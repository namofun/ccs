namespace Ccs.Models
{
    /// <summary>
    /// The model class for team members.
    /// </summary>
    public class TeamMemberModel
    {
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
        /// Instantiate the <see cref="TeamMemberModel"/>.
        /// </summary>
        public TeamMemberModel(int userId, string userName, string? lastLoginIp)
        {
            UserId = userId;
            UserName = userName;
            LastLoginIp = lastLoginIp;
        }
    }
}
