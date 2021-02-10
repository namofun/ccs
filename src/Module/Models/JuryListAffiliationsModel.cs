using System.Collections;
using System.Collections.Generic;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryListAffiliationsModel : IEnumerable<Affiliation>
    {
        IEnumerator<Affiliation> IEnumerable<Affiliation>.GetEnumerator() => Affiliations.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Affiliations.Values.GetEnumerator();

        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; set; }

        public Dictionary<int, int> TeamCount { get; set; }

        public HashSet<int> AllowedTenants { get; set; }
    }
}
