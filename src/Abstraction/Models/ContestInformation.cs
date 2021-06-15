namespace Ccs.Models
{
    /// <summary>
    /// The contest information.
    /// </summary>
    public interface IContestInformation : IContestTime
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The contest title
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The contest short name
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Whether this contest is public
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// The ranking strategy
        /// </summary>
        /// <remarks>
        /// This represent the kind of ranking strategy.
        /// <list type="bullet"><c>0</c>: XCPC Rules</list>
        /// <list type="bullet"><c>1</c>: IOI Rules</list>
        /// <list type="bullet"><c>2</c>: Codeforces Rules</list>
        /// </remarks>
        int RankingStrategy { get; }

        /// <summary>
        /// The flags for contest kind
        /// </summary>
        /// <remarks>
        /// This represent the kind of contest.
        /// <list type="bullet"><c>0</c>: Normal contest with DOMjudge UI</list>
        /// <list type="bullet"><c>1</c>: Practice contest with Codeforces UI</list>
        /// <list type="bullet"><c>2</c>: Problem set with legacy OJ UI</list>
        /// </remarks>
        int Kind { get; }

        /// <summary>
        /// The contest settings
        /// </summary>
        IContestSettings Settings { get; }

        /// <summary>
        /// Checks whether the scoreboard paging should be enabled.
        /// </summary>
        /// <returns>The value indicating that.</returns>
        bool ShouldScoreboardPaging();
    }
}
