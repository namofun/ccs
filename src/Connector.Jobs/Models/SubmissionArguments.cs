namespace Ccs.Connector.Jobs.Models
{
    public class SubmissionArguments : ScoreboardArguments
    {
        public bool IncludePrevious { get; set; }

        public bool IncludeInner { get; set; }

        public int[] FilteredProblems { get; set; }
    }
}
