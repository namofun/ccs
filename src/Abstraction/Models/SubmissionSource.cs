namespace Ccs.Models
{
    public class SubmissionSource
    {
        public string Language { get; }

        public string SourceCode { get; }

        public int Id { get; }

        public int ContestId { get; }

        public int TeamId { get; }

        public int ProblemId { get; }

        public SubmissionSource(int id, int cid, int teamid, int probid, string langId, string source)
        {
            Language = langId;
            SourceCode = source;
            Id = id;
            ContestId = cid;
            TeamId = teamid;
            ProblemId = probid;
        }
    }
}
