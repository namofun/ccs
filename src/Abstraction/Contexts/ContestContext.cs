using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// The contest entity
        /// </summary>
        Contest Contest { get; }

        /// <summary>
        /// Fetch the ID of the latest event.
        /// </summary>
        /// <returns>The task for ID of event.</returns>
        Task<int> MaxEventIdAsync();

        /// <summary>
        /// Fetch the event for a contest after some ID.
        /// </summary>
        /// <param name="type">The endpoint type.</param>
        /// <param name="after">The first event ID.</param>
        /// <returns>The task for fetching new events.</returns>
        Task<List<Event>> FetchEventAsync(string[]? type = null, int after = 0);

        /// <summary>
        /// Get the status of judge queue.
        /// </summary>
        /// <returns>The list of judge queues.</returns>
        Task<Polygon.Models.ServerStatus> GetJudgeQueueAsync();

        /// <summary>
        /// Find the submission.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <param name="includeJudgings">Whether to include judgings in results.</param>
        /// <returns>The task for fetching submission entity.</returns>
        Task<Submission?> FindSubmissionAsync(int submissionId, bool includeJudgings = false);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="all">Whether to show all solutions.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<Polygon.Models.Solution>> FetchSolutionsAsync(int? probid = null, string? langid = null, int? teamid = null, bool all = false);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The count per page.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<IPagedList<Polygon.Models.Solution>> FetchSolutionsAsync(int page, int perPage);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="submitid">The submission ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<Polygon.Models.Solution> FetchSolutionAsync(int submitid);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<TSolution>> FetchSolutionsAsync<TSolution>(Expression<Func<Submission, Judging, TSolution>> selector, int? probid = null, string? langid = null, int? teamid = null);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="submitid">The submission ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<TSolution> FetchSolutionAsync<TSolution>(int submitid, Expression<Func<Submission, Judging, TSolution>> selector);

        /// <summary>
        /// List the submissions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="projection">The entity shaper.</param>
        /// <param name="predicate">The submission filter.</param>
        /// <returns>The submission list.</returns>
        Task<List<T>> ListSubmissionsAsync<T>(
            Expression<Func<Submission, T>> projection,
            Expression<Func<Submission, bool>>? predicate = null);

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
        /// Fetch the affiliation.
        /// </summary>
        /// <param name="id">The affiliation ID.</param>
        /// <returns>The task for fetching affiliations.</returns>
        Task<Affiliation?> FetchAffiliationAsync(int id);

        /// <summary>
        /// Fetch the affiliations used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching affiliations.</returns>
        Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool contestFiltered = true);

        /// <summary>
        /// Fetch the categories used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching affiliations.</returns>
        Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool contestFiltered = true);

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
        /// Fetch the team members as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this lookup.</returns>
        Task<ILookup<int, string>> FetchTeamMembersAsync();

        /// <summary>
        /// Fetch the team member.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <returns>The task for getting member.</returns>
        Task<IEnumerable<string>> FetchTeamMemberAsync(Team team);

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
        /// Create team.
        /// </summary>
        /// <param name="team">The original team model.</param>
        /// <param name="users">The team members.</param>
        /// <returns>The task for creating contest teams, returning the team id.</returns>
        Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users);

        /// <summary>
        /// Update team by expression.
        /// </summary>
        /// <param name="origin">The original team model.</param>
        /// <param name="expression">The expression for updating team.</param>
        /// <returns>The task for updating contest teams.</returns>
        Task UpdateTeamAsync(Team origin, Expression<Func<Team>> expression);

        /// <summary>
        /// Delete the team and returns existing members.
        /// </summary>
        /// <param name="origin">The original team.</param>
        /// <returns>The task for deleting contest teams.</returns>
        Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin);

        /// <summary>
        /// List the teams with selected conditions.
        /// </summary>
        /// <typeparam name="T">The selected entity type.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="predicate">The conditions to match.</param>
        /// <returns>The task for listing entities.</returns>
        Task<List<T>> ListTeamsAsync<T>(
            Expression<Func<Team, T>> selector,
            Expression<Func<Team, bool>>? predicate = null)
            where T : class;

        /// <summary>
        /// List the clarifications via predicate.
        /// </summary>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The task for listing clarifications.</returns>
        Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate);

        /// <summary>
        /// Find the clarification via ID.
        /// </summary>
        /// <param name="id">The clarification ID.</param>
        /// <returns>The task for fetching clarification.</returns>
        Task<Clarification> FindClarificationAsync(int id);

        /// <summary>
        /// Send a clarification.
        /// </summary>
        /// <param name="clar">The new clarification to create.</param>
        /// <param name="replyTo">The clarification to reply to.</param>
        /// <returns>The task for creating clarification.</returns>
        Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null);

        /// <summary>
        /// Get the readme content.
        /// </summary>
        /// <param name="source">The source code.</param>
        /// <returns>The task for reading me.</returns>
        Task<string> GetReadmeAsync(bool source = false);

        /// <summary>
        /// Get the readme content.
        /// </summary>
        /// <param name="source">The original source code.</param>
        /// <returns>The task for saving readme.</returns>
        Task SetReadmeAsync(string source);

        /// <summary>
        /// Get the auditlogs.
        /// </summary>
        /// <param name="page">The page to show.</param>
        /// <param name="pageCount">The count of pages to show.</param>
        /// <returns>The task with auditlogs.</returns>
        Task<IPagedList<SatelliteSite.Entities.Auditlog>> ViewLogsAsync(int page, int pageCount);

        /// <summary>
        /// Get the ajax update overview.
        /// </summary>
        /// <returns>The object for update results.</returns>
        Task<object> GetUpdatesAsync();

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
