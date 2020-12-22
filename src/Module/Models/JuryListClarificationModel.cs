using Ccs.Entities;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryListClarificationModel
    {
        public List<Clarification> AllClarifications { get; set; }

        public IReadOnlyList<ContestProblem> Problems { get; set; }

        public IReadOnlyDictionary<int, string> TeamNames { get; set; }

        public string JuryName { get; set; }

        public IEnumerable<Clarification> NewRequests =>
            AllClarifications.Where(c => !c.Answered && c.Sender != null);

        public IEnumerable<Clarification> OldRequests =>
            AllClarifications.Where(c => c.Answered && c.Sender != null);

        public IEnumerable<Clarification> GeneralClarifications =>
            AllClarifications.Where(c => c.Sender == null);
    }
}
