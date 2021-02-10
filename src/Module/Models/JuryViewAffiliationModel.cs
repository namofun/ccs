using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewAffiliationModel
    {
        public Affiliation Affiliation { get; set; }

        public List<Team> Teams { get; set; }
    }
}
