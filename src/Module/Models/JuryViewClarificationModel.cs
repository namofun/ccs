using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewClarificationModel
    {
        public Clarification Main { get; set; }

        public IEnumerable<Clarification> Associated { get; set; }

        public IReadOnlyDictionary<int, string> Teams { get; set; }

        public IReadOnlyList<ProblemModel> Problems { get; set; }

        public string UserName { get; set; }
    }
}
