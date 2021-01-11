using System;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryListTeamModel
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public string Category { get; set; }

        public string Affiliation { get; set; }

        public string AffiliationName { get; set; }

        public int Status { get; set; }

        public DateTimeOffset? RegisterTime { get; set; }
    }
}
