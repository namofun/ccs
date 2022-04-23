using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewAffiliationModel
    {
        public Affiliation Affiliation { get; set; }

        public List<Team> Teams { get; set; }
    }
}
