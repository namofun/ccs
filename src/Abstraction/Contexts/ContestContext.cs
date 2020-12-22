using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context should be constructed by <see cref="Contexts.IContestContextFactory"/>.
    /// </para>
    /// </remarks>
    public interface IContestContext
    {
        /// <summary>
        /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>A service object of type T.</returns>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
        T GetRequiredService<T>();

        /// <summary>
        /// The contest entity
        /// </summary>
        Contest Contest { get; }

        /// <summary>
        /// Asynchronously update the contest.
        /// </summary>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>The task for updating the contest.</returns>
        Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression);

        /// <summary>
        /// Fetch the contest scoreboard.
        /// </summary>
        /// <returns>The task for fetching scoreboard model.</returns>
        Task<ScoreboardModel> FetchScoreboardAsync();

        /// <summary>
        /// Fetch the available languages.
        /// </summary>
        /// <returns>The task for fetching languages.</returns>
        Task<IReadOnlyList<Language>> FetchLanguagesAsync();

        /// <summary>
        /// Fetch the contest problems.
        /// </summary>
        /// <returns>The task for fetching problems.</returns>
        Task<IReadOnlyList<ProblemModel>> FetchProblemsAsync();

        /// <summary>
        /// Fetch the affiliations used in contest.
        /// </summary>
        /// <returns>The task for fetching affiliations.</returns>
        Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync();

        /// <summary>
        /// Fetch the categories used in contest.
        /// </summary>
        /// <returns>The task for fetching affiliations.</returns>
        Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync();

        /// <summary>
        /// Find team by team ID.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindTeamByIdAsync(int teamId);

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for fetching team member entity.</returns>
        Task<Member?> FindMemberByUserAsync(int userId);

        /// <summary>
        /// Fetch the team names as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this dictionary.</returns>
        Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync();

        /// <summary>
        /// Fetch the team names as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this dictionary.</returns>
        Task<IReadOnlyDictionary<int, (string Name, string Affiliation)>> FetchPublicTeamNamesWithAffiliationAsync();

        /// <summary>
        /// Get the jury list.
        /// </summary>
        /// <returns>The task for fetching jury list.</returns>
        /// <remarks>Implementation should take a <see cref="HashSet{T}"/> as resulting type.</remarks>
        Task<HashSet<int>> FetchJuryAsync();

        /// <summary>
        /// Assign jury user to contest.
        /// </summary>
        /// <param name="user">The jury user.</param>
        /// <returns>The task for assigning jury.</returns>
        Task AssignJuryAsync(IUser user);

        /// <summary>
        /// Unassign jury user to contest.
        /// </summary>
        /// <param name="user">The jury user.</param>
        /// <returns>The task for assigning jury.</returns>
        Task UnassignJuryAsync(IUser user);

        /// <summary>
        /// Create problems by expression.
        /// </summary>
        /// <param name="expression">The expression for creating contest problem.</param>
        /// <returns>The task for creating contest problems.</returns>
        Task CreateProblemAsync(Expression<Func<ContestProblem>> expression);

        /// <summary>
        /// Update problems by expression.
        /// </summary>
        /// <param name="origin">The original problem model.</param>
        /// <param name="expression">The expression for updating contest problem.</param>
        /// <returns>The task for updating contest problems.</returns>
        Task UpdateProblemAsync(ProblemModel origin, Expression<Func<ContestProblem>> expression);

        /// <summary>
        /// Delete such problem from contest.
        /// </summary>
        /// <param name="problem">The original problem model.</param>
        /// <returns>The task for deleting contest problems.</returns>
        Task DeleteProblemAsync(ProblemModel problem);

        /// <summary>
        /// Update team by expression.
        /// </summary>
        /// <param name="origin">The original team model.</param>
        /// <param name="expression">The expression for updating team.</param>
        /// <returns>The task for updating contest teams.</returns>
        Task UpdateTeamAsync(Team origin, Expression<Func<Team>> expression);

        /// <summary>
        /// Create a submission for team.
        /// </summary>
        /// <param name="code">The source code.</param>
        /// <param name="language">The language.</param>
        /// <param name="problem">The problem.</param>
        /// <param name="team">The team.</param>
        /// <param name="ipAddr">The IP Address.</param>
        /// <param name="via">The submission source.</param>
        /// <param name="username">The submission username.</param>
        /// <param name="time">The submission time.</param>
        /// <returns>The task for creating submission.</returns>
        Task<Submission> SubmitAsync(
            string code,
            Language language,
            ContestProblem problem,
            Team team,
            IPAddress ipAddr,
            string via,
            string username,
            DateTimeOffset? time = null);
    }
}
