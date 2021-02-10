using Ccs.Entities;
using System.Collections.Generic;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewCategoryModel
    {
        public Category Category { get; set; }

        public List<Team> Teams { get; set; }
    }
}
