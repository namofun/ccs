namespace Ccs.Connector.Jobs.Models
{
    public class ScoreboardArguments
    {
        public int[] FilteredAffiliations { get; set; }

        public int[] FilteredCategories { get; set; }

        public int ContestId { get; set; }
    }
}
