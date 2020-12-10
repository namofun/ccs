namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contest problem.
    /// </summary>
    public class ContestProblem
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The navigation to contest
        /// </summary>
        public Contest Contest { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The navigation to problem
        /// </summary>
        public Polygon.Entities.Problem Problem { get; set; }

        /// <summary>
        /// The short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Whether this problem allow submit
        /// </summary>
        public bool AllowSubmit { get; set; }

        /// <summary>
        /// The balloon color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// The full score in Codeforces Mode
        /// </summary>
        public int Score { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="ContestProblem"/>.
        /// </summary>
        public ContestProblem()
        {
        }
#pragma warning restore CS8618
    }
}
