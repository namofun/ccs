#nullable disable

namespace Ccs.Models
{
    public class TeamModel
    {
        public int? ContestId { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public string Affiliation { get; set; }

        public string AffiliationId { get; set; }

        public string Category { get; set; }

        public string CategoryColor { get; set; }

        public bool Eligible { get; set; }

        public int? Rank { get; set; }

        public bool ShowRank { get; set; }

        public int Points { get; set; }

        public int Penalty { get; set; }

        public int LastAc { get; set; }

        public ScoreCellModel[] Problems { get; set; }
    }
}
