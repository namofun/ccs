#nullable enable
using Ccs.Contexts;
using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs
{
    /// <summary>
    /// Extensions for contest context.
    /// </summary>
    public static class ContestContextExtensions
    {
        /// <summary>
        /// Find the contest problem with corresponding problem ID.
        /// </summary>
        /// <param name="problems">The contest problems.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public static ProblemModel? Find(this IReadOnlyList<ProblemModel> problems, int probid)
        {
            return problems.FirstOrDefault(p => p.ProblemId == probid);
        }

        /// <summary>
        /// Find the contest problem with corresponding problem short name.
        /// </summary>
        /// <param name="problems">The contest problems.</param>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public static ProblemModel? Find(this IReadOnlyList<ProblemModel> problems, string shortName)
        {
            return problems.FirstOrDefault(p => p.ShortName == shortName);
        }

        /// <summary>
        /// Validate whether this user has permission to the contest jury part.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="user">The user claims principal.</param>
        /// <returns>The task for validation, <c>true</c> for jury and <c>false</c> for non-jury.</returns>
        public static async Task<bool> ValidateAsync(this IContestContext context, ClaimsPrincipal user)
        {
            if (user.IsInRole("Administrator")) return true;
            if (!int.TryParse(user.GetUserId(), out var uid))
                return false;
            var jury = await context.FetchJuryAsync();
            return jury.Contains(uid);
        }

        /// <summary>
        /// Create a <see cref="IContestContext"/> via the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <param name="contestId">The contest ID.</param>
        /// <returns>The task for creating context.</returns>
        public static Task<IContestContext?> CreateContestContextAsync(
            this HttpContext httpContext, int contestId)
        {
            return httpContext.RequestServices
                .GetRequiredService<ScopedContestContextFactory>()
                .CreateAsync(contestId);
        }

        /// <summary>
        /// Get the linked contest context.
        /// </summary>
        /// <param name="httpContext">The http context.</param>
        /// <returns>The contest context, or throwing an exception.</returns>
        /// <exception cref="InvalidOperationException">Occurs when no <see cref="IContestContext"/> was linked.</exception>
        public static IContestContext GetContestContext(this HttpContext httpContext)
        {
            return httpContext.Features.Get<IContestContext>()
                ?? throw new InvalidOperationException("No contest context is linked with this http context.");
        }

        /// <summary>
        /// Get the clarification categories from contest problems.
        /// </summary>
        /// <param name="problems">The contest problems.</param>
        /// <returns>The enumerable for tuple (CategoryName, CategoryEnum, ProblemId).</returns>
        public static IEnumerable<(string, ClarificationCategory, int?)> GetClarificationCategories(this IReadOnlyList<ProblemModel> problems)
        {
            return problems
                .Select(cp => ($"prob-{cp.ShortName}", ClarificationCategory.Problem, (int?)cp.ProblemId))
                .Prepend(("tech", ClarificationCategory.Technical, null))
                .Prepend(("general", ClarificationCategory.General, null));
        }

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        public static async Task<Team?> FindTeamByUserAsync(this IContestContext context, int userId)
        {
            var member = await context.FindMemberByUserAsync(userId);
            if (member == null) return null;
            return await context.FindTeamByIdAsync(member.TeamId);
        }

        /// <summary>
        /// Fetch the contest problem by ID.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The task for fetching problems.</returns>
        public static async Task<ProblemModel?> FindProblemAsync(this IContestContext context, int probid)
        {
            var probs = await context.FetchProblemsAsync();
            return probs.Find(probid);
        }

        /// <summary>
        /// Fetch the contest problem by short name.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The task for fetching problems.</returns>
        public static async Task<ProblemModel?> FindProblemAsync(this IContestContext context, string shortName)
        {
            var probs = await context.FetchProblemsAsync();
            return probs.Find(shortName);
        }

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="all">Whether to show all solutions.</param>
        /// <returns>The task for fetching solution list.</returns>
        public static Task<List<Solution>> FetchSolutionsAsync(
            this IContestContext context,
            int? probid = null,
            string? langid = null,
            int? teamid = null,
            bool all = false)
        {
            int cid = context.Contest.Id;
            var cond = Expr
                .Create<Submission>(s => s.ContestId == cid)
                .CombineIf(probid.HasValue, s => s.ProblemId == probid)
                .CombineIf(teamid.HasValue, s => s.TeamId == teamid)
                .CombineIf(!string.IsNullOrEmpty(langid), s => s.Language == langid);
            int? limit = all ? default(int?) : 75;

            return context
                .GetRequiredService<ISubmissionStore>()
                .ListWithJudgingAsync(cond, true, limit);
        }
    }
}
