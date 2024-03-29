﻿using System;

namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for contest teams.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The team name
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// The affiliation ID
        /// </summary>
        public int AffiliationId { get; set; }

        /// <summary>
        /// The category ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The team status
        /// </summary>
        /// <remarks>
        /// This represent the status of team.
        /// <list type="bullet"><c>0</c>: Pending for justification</list>
        /// <list type="bullet"><c>1</c>: Accepted</list>
        /// <list type="bullet"><c>2</c>: Rejected</list>
        /// <list type="bullet"><c>3</c>: Deleted internally</list>
        /// </remarks>
        public int Status { get; set; }

        /// <summary>
        /// The team register time
        /// </summary>
        public DateTimeOffset? RegisterTime { get; set; }

        /// <summary>
        /// The team contest time
        /// </summary>
        public DateTimeOffset? ContestTime { get; set; }

        /// <summary>
        /// The location of team
        /// </summary>
        public string? Location { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="Team"/>.
        /// </summary>
        public Team()
        {
        }
#pragma warning restore CS8618
    }
}
