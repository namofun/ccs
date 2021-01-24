using System.ComponentModel;

namespace Ccs.Registration.BatchByTeamName
{
    public class BatchByTeamNameInputModel
    {
        [DisplayName("Affiliation")]
        public int AffiliationId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [DisplayName("Team Names")]
        public string TeamNames { get; set; }
    }
}
