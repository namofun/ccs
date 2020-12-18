using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for teams to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class TeamsController : ApiControllerBase
    {
        /// <summary>
        /// Get all the teams for this contest
        /// </summary>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="cid">The contest ID</param>
        /// <param name="category">Only show teams for the given category</param>
        /// <param name="affiliation">Only show teams for the given affiliation / organization</param>
        /// <param name="public">Only show visible teams, even for users with more permissions</param>
        /// <response code="200">Returns all the teams for this contest</response>
        [HttpGet]
        public async Task<ActionResult<List<Team>>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] int? category = null,
            [FromQuery] string affiliation = null,
            [FromQuery] bool @public = false)
        {
            var cond = Expr
                .Create<Ccs.Entities.Team>(t => t.ContestId == cid && t.Status == 1)
                .CombineIf(category.HasValue, t => t.CategoryId == category)
                .CombineIf(ids != null && ids.Length > 0, t => ids.Contains(t.TeamId))
                .CombineIf(affiliation != null, t => t.Affiliation.Abbreviation == affiliation)
                .CombineIf(@public, t => t.Category.IsPublic);

            var store = Context.GetRequiredService<ITeamStore>();
            return await store.ListAsync(t => new Team(t, t.Affiliation), cond);
        }


        /// <summary>
        /// Get the given team for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given team for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var team = await Context.FindTeamByIdAsync(id);
            var affs = await Context.FetchAffiliationsAsync();
            return new Team(team, affs[team.AffiliationId]);
        }
    }
}
