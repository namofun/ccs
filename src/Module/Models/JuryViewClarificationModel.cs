using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewClarificationModel
    {
        public Clarification Main { get; set; }

        public IEnumerable<Clarification> Associated { get; set; }

        public IReadOnlyDictionary<int, string> Teams { get; set; }

        public ProblemCollection Problems { get; set; }

        public string UserName { get; set; }
    }
}
