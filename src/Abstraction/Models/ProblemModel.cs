namespace Ccs.Models
{
    /// <summary>
    /// The model class for contest problems.
    /// </summary>
    public class ProblemModel : Entities.ContestProblem
    {
        /// <summary>
        /// The rank order (starting from 1)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Whether this problem allow judge
        /// </summary>
        public bool AllowJudge { get; }

        /// <summary>
        /// The problem title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The time limit
        /// </summary>
        public int TimeLimit { get; }

        /// <summary>
        /// The memory limit
        /// </summary>
        public int MemoryLimit { get; }

        /// <summary>
        /// The count of testcases
        /// </summary>
        public int TestcaseCount { get; set; }

        /// <summary>
        /// Whether this is interactive problem
        /// </summary>
        public bool Interactive { get; }

        /// <summary>
        /// Whether this problem is shared
        /// </summary>
        public bool Shared { get; }

        /// <summary>
        /// The problem statement
        /// </summary>
        public string? Statement { get; set; }

        /// <summary>
        /// The problem tag
        /// </summary>
        public string TagName { get; }

        /// <summary>
        /// The problem source
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Instantiate a model for contest problem.
        /// </summary>
        public ProblemModel(
            int cid, int probid, string shortName,
            bool allowSubmit, bool allowJudge,
            string color, int codeforcesScore, string tag, string source,
            string title, int timelimit, int memlimit, bool interactive, bool shared)
        {
            AllowJudge = allowJudge;
            AllowSubmit = allowSubmit;
            Color = color;
            ContestId = cid;
            Interactive = interactive;
            TimeLimit = timelimit;
            MemoryLimit = memlimit;
            ProblemId = probid;
            Score = codeforcesScore;
            Shared = shared;
            ShortName = shortName;
            Title = title;
            TagName = tag;
            Source = source;
        }
    }
}
