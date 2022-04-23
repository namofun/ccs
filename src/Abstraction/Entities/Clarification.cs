using System;

namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for clarifications.
    /// </summary>
    public class Clarification
    {
        /// <summary>
        /// The clarification ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The ID of clarification responded to
        /// </summary>
        public int? ResponseToId { get; set; }

        /// <summary>
        /// The submit time
        /// </summary>
        public DateTimeOffset SubmitTime { get; set; }

        /// <summary>
        /// The sender team ID
        /// </summary>
        public int? Sender { get; set; }

        /// <summary>
        /// The recipient team ID
        /// </summary>
        public int? Recipient { get; set; }

        /// <summary>
        /// The jury member processing this clarification
        /// </summary>
        public string? JuryMember { get; set; }

        /// <summary>
        /// The category of issues
        /// </summary>
        public ClarificationCategory Category { get; set; }

        /// <summary>
        /// The problem ID when problem issues
        /// </summary>
        public int? ProblemId { get; set; }

        /// <summary>
        /// The body text
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Whether this clarification has been answered
        /// </summary>
        public bool Answered { get; set; }

        /// <summary>
        /// Check whether this team has permission to view this ticket.
        /// </summary>
        /// <param name="teamid">The viewing team ID.</param>
        public bool CheckPermission(int teamid)
        {
            return !Recipient.HasValue || Recipient == teamid || Sender == teamid;
        }
    }
}
