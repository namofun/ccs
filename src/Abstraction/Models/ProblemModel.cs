namespace Ccs.Models
{
    /// <summary>
    /// The model class for contest problems.
    /// </summary>
    public class ProblemModel
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; }

        /// <summary>
        /// The short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// The rank order (starting from 1)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Whether this problem allow submit
        /// </summary>
        public bool AllowSubmit { get; set; }

        /// <summary>
        /// Whether this problem allow judge
        /// </summary>
        public bool AllowJudge { get; }

        /// <summary>
        /// The balloon color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// The full score in Codeforces Mode
        /// </summary>
        public int Score { get; set; }

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
        public int TestcaseCount { get; }

        /// <summary>
        /// Whether this is interactive problem
        /// </summary>
        public bool Interactive { get; }

        /// <summary>
        /// Whether this problem is shared
        /// </summary>
        public bool Shared { get; }

        /// <summary>
        /// Instantiate a model for contest problem.
        /// </summary>
        public ProblemModel(int cid, int probid, string shortName, bool allowSubmit, bool allowJudge, string color, int codeforcesScore, string title, int timelimit, int memlimit, bool interactive, bool shared, int rank)
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
            Rank = rank;
        }
    }
}
