namespace Ccs.Models
{
    /// <summary>
    /// The model class for contest problems.
    /// </summary>
    public class Problem2Model : Entities.ContestProblem
    {
        /// <summary>
        /// The contest name
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The contest big title
        /// </summary>
        public string BigTitle { get; }

        /// <inheritdoc cref="Ccs.Entities.Contest.Kind"/>
        public int Kind { get; }

        /// <inheritdoc cref="Ccs.Entities.Contest.RankingStrategy"/>
        public int RankStrategy { get; }

        /// <summary>
        /// Instantiate a model for contest problem.
        /// </summary>
        public Problem2Model(int cid, int probid, string shortName, bool allowSubmit, string color, int codeforcesScore, string name, string bigTitle, int kind, int rankStrategy)
        {
            AllowSubmit = allowSubmit;
            Color = color;
            ContestId = cid;
            Kind = kind;
            RankStrategy = rankStrategy;
            ProblemId = probid;
            Score = codeforcesScore;
            ShortName = shortName;
            Title = name;
            BigTitle = bigTitle;
        }
    }
}
