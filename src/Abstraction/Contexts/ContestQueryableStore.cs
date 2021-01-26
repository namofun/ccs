using Ccs.Entities;
using System.Linq;

namespace Ccs.Services
{
    /// <summary>
    /// The marker interface to gets the <see cref="IQueryable{T}"/>s.
    /// </summary>
    public interface IContestQueryableStore
    {
        /// <summary>
        /// Gets the queryable for <see cref="Contest"/>.
        /// </summary>
        IQueryable<Contest> Contests { get; }

        /// <summary>
        /// Gets the queryable for <see cref="ContestProblem"/>.
        /// </summary>
        IQueryable<ContestProblem> ContestProblems { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Jury"/>.
        /// </summary>
        IQueryable<Jury> ContestJuries { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Team"/>.
        /// </summary>
        IQueryable<Team> Teams { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Member"/>.
        /// </summary>
        IQueryable<Member> TeamMembers { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Clarification"/>.
        /// </summary>
        IQueryable<Clarification> Clarifications { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Balloon"/>.
        /// </summary>
        IQueryable<Balloon> Balloons { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Event"/>.
        /// </summary>
        IQueryable<Event> ContestEvents { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Printing"/>.
        /// </summary>
        IQueryable<Printing> Printings { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Entities.RankCache"/>.
        /// </summary>
        IQueryable<RankCache> RankCache { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Entities.ScoreCache"/>.
        /// </summary>
        IQueryable<ScoreCache> ScoreCache { get; }
    }
}
