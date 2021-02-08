using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Registration
{
    /// <summary>
    /// The execution context for register provider.
    /// </summary>
    public sealed class RegisterProviderContext
    {
        private IUserManager? _userManager;
        private readonly ITeamContext _teamContext;

        /// <summary>
        /// Provides user management.
        /// </summary>
        public IUserManager UserManager
            => _userManager ??= GetRequiredService<IUserManager>();

        /// <summary>
        /// Provides http context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Provides claims principal.
        /// </summary>
        public ClaimsPrincipal User
            => HttpContext.User;

        /// <summary>
        /// Gets the required service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The service instance.</returns>
        public TService GetRequiredService<TService>()
            => HttpContext.RequestServices.GetRequiredService<TService>();

        /// <summary>
        /// Provides contest feature.
        /// </summary>
        public IContestContextAccessor Contest { get; }

        /// <summary>
        /// Provides contest team.
        /// </summary>
        public Team? Team => Contest.Team;

        /// <summary>
        /// Create team.
        /// </summary>
        /// <param name="team">The original team model.</param>
        /// <param name="users">The team members.</param>
        /// <returns>The task for creating contest teams, returning the team id.</returns>
        public Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
            => _teamContext.CreateTeamAsync(team, users);

        /// <summary>
        /// Fetch the affiliations used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching affiliations.</returns>
        public Task<IReadOnlyDictionary<int, Affiliation>> ListAffiliationsAsync(bool contestFiltered = true)
            => _teamContext.ListAffiliationsAsync(contestFiltered);

        /// <summary>
        /// Fetch the categories used in contest.
        /// </summary>
        /// <param name="contestFiltered">Whether filtering the entities only used in this contest.</param>
        /// <returns>The task for fetching affiliations.</returns>
        public Task<IReadOnlyDictionary<int, Category>> ListCategoriesAsync(bool contestFiltered = true)
            => _teamContext.ListCategoriesAsync(contestFiltered);

        /// <summary>
        /// Attach a user to the team if not attached.
        /// </summary>
        /// <param name="team">The contest team.</param>
        /// <param name="user">The identity user.</param>
        /// <param name="temporary">Whether this member is temporary account.</param>
        /// <returns>The task for attaching member.</returns>
        public Task AttachMemberAsync(Team team, IUser user, bool temporary)
            => _teamContext.AttachMemberAsync(team, user, temporary);

        /// <summary>
        /// Fetch the team members as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this lookup.</returns>
        public Task<ILookup<int, string>> GetTeamMembersAsync()
            => _teamContext.GetTeamMembersAsync();

        /// <summary>
        /// List the teams with selected conditions.
        /// </summary>
        /// <param name="predicate">The conditions to match.</param>
        /// <returns>The task for listing entities.</returns>
        public Task<List<Team>> ListTeamsAsync(Expression<Func<Team, bool>>? predicate = null)
            => _teamContext.ListTeamsAsync(predicate);

        /// <summary>
        /// Instantiate an execution context for register provider.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="httpContext">The http context.</param>
        public RegisterProviderContext(IContestContextAccessor context, HttpContext httpContext, IUserManager? userManager = null)
        {
            Contest = context;
            _userManager = userManager;
            _teamContext = (context.Context as ITeamContext)
                ?? throw new NotSupportedException("Team controlling is not supported.");
            HttpContext = httpContext;
        }
    }
}
