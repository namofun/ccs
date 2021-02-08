﻿using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context should be constructed by <see cref="IContestContextFactory"/>.
    /// </para>
    /// </remarks>
    public interface IContestContext
    {
        /// <summary>
        /// The contest entity
        /// </summary>
        IContestInformation Contest { get; }

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
        /// Finds the specified language.
        /// </summary>
        /// <returns>The task for finding language.</returns>
        Task<Language?> FindLanguageAsync(string? langid);

        /// <summary>
        /// Lists the contest problems.
        /// </summary>
        /// <param name="nonCached">Whether to fetch the non-cached result.</param>
        /// <returns>The task for fetching problems.</returns>
        Task<ProblemCollection> ListProblemsAsync(bool nonCached = false);

        /// <summary>
        /// Finds the problem in current problemset.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="withStatement">Whether to include statement.</param>
        /// <returns>The task for fetching model.</returns>
        Task<ProblemModel?> FindProblemAsync(string probid, bool withStatement = false);

        /// <summary>
        /// Finds the problem in current problemset.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="withStatement">Whether to include statement.</param>
        /// <returns>The task for fetching model.</returns>
        Task<ProblemModel?> FindProblemAsync(int probid, bool withStatement = false);

        /// <summary>
        /// Gets all the affiliations used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching affiliations.</returns>
        Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool contestFiltered = true);

        /// <summary>
        /// Gets all the categories used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching categories.</returns>
        Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool contestFiltered = true);

        /// <summary>
        /// Gets all the team names as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this dictionary.</returns>
        Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync();

        /// <summary>
        /// Finds team by team ID.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindTeamByIdAsync(int teamId);

        /// <summary>
        /// Finds team by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for fetching team member entity.</returns>
        Task<Member?> FindMemberByUserAsync(int userId);

        /// <summary>
        /// Get the readme content.
        /// </summary>
        /// <param name="source">The source code.</param>
        /// <returns>The task for reading me.</returns>
        Task<string> GetReadmeAsync(bool source = false);

        /// <summary>
        /// Get the jury list.
        /// </summary>
        /// <returns>The task for fetching jury list.</returns>
        Task<Dictionary<int, string>> FetchJuryAsync();

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<TSolution>> FetchSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            int? probid = null, string? langid = null, int? teamid = null);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="submitid">The submission ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<TSolution> FetchSolutionAsync<TSolution>(
            int submitid,
            Expression<Func<Submission, Judging, TSolution>> selector);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="all">Whether to show all solutions.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<Polygon.Models.Solution>> FetchSolutionsAsync(
            int? probid = null, string? langid = null, int? teamid = null, bool all = false);

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