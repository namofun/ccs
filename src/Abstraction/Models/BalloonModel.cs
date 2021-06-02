using System;

namespace Ccs.Models
{
    /// <summary>
    /// The model class for balloons.
    /// </summary>
    public class BalloonModel
    {
        /// <summary>
        /// The balloon ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// Whether the balloon has been handed out
        /// </summary>
        public bool Done { get; }

        /// <summary>
        /// The color of balloon
        /// </summary>
        /// <remarks>
        /// This property should be set when sending to views.
        /// </remarks>
        public string? BalloonColor { get; set; }

        /// <summary>
        /// The short name of problem
        /// </summary>
        /// <remarks>
        /// This property should be set when sending to views.
        /// </remarks>
        public string? ProblemShortName { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; }

        /// <summary>
        /// The team name
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// The name of team affiliation
        /// </summary>
        public string AffiliationName { get; set; }

        /// <summary>
        /// The short name of team affiliation
        /// </summary>
        public string AffiliationShortName { get; set; }

        /// <summary>
        /// The name of team category
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Whether team is first to solve this problem
        /// </summary>
        public bool FirstToSolve { get; set; }

        /// <summary>
        /// The submission time
        /// </summary>
        public DateTimeOffset Time { get; }

        /// <summary>
        /// The location of team
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The sort order for this team
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// The previous balloon
        /// </summary>
        public BalloonModel? Previous { get; set; }

        /// <summary>
        /// Instantiate a model for balloons.
        /// </summary>
        /// <remarks>
        /// When sending to views, please fill the field of <see cref="ProblemShortName"/> and <see cref="BalloonColor"/>.
        /// </remarks>
        /// <param name="bid">The balloon ID.</param>
        /// <param name="submitid">The submission ID.</param>
        /// <param name="done">Whether has been sent.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="time">The submit time.</param>
        public BalloonModel(int bid, int submitid, bool done, int probid, int teamid, DateTimeOffset time)
        {
            Id = bid;
            SubmissionId = submitid;
            Done = done;
            TeamId = teamid;
            TeamName = $"t{teamid}";
            Time = time;
            ProblemId = probid;
            Location = string.Empty;
            CategoryName = "Unknown";
            AffiliationName = "Unknown";
            AffiliationShortName = "UKE";
        }
    }
}
