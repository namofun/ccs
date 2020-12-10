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
        public string TeamName { get; }

        /// <summary>
        /// The name of team category
        /// </summary>
        public string CategoryName { get; }

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
        public string Location { get; }

        /// <summary>
        /// The sort order for this team
        /// </summary>
        public int SortOrder { get; }

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
        /// <param name="teamName">The team name.</param>
        /// <param name="teamLoc">The team location.</param>
        /// <param name="time">The submit time.</param>
        /// <param name="catName">The category name.</param>
        /// <param name="sortOrder">The sort order.</param>
        public BalloonModel(
            int bid, int submitid, bool done,
            int probid, int teamid, string teamName, string teamLoc,
            DateTimeOffset time, string catName, int sortOrder)
        {
            Id = bid;
            SubmissionId = submitid;
            Done = done;
            TeamName = $"t{teamid}: {teamName}";
            CategoryName = catName;
            Time = time;
            ProblemId = probid;
            Location = teamLoc;
            SortOrder = sortOrder;
        }
    }
}
