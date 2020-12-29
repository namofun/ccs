using System.ComponentModel;

namespace SatelliteSite.ContestModule.Models
{
    public class AddTeamByListModel
    {
        [DisplayName("Affiliation")]
        public int AffiliationId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [DisplayName("Team Names")]
        public string TeamNames { get; set; }
    }
}
