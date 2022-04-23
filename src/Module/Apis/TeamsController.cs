using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Services;
using Xylab.Contesting.Specifications;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for teams to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class TeamsController : ApiControllerBase<ITeamContext>
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
            var affs = await Context.ListAffiliationsAsync();
            var cats = await Context.ListCategoriesAsync();
            int? affId = null;

            if (affiliation != null)
            {
                // Assume that all abbr. is unique
                var aff = affs.Values.FirstOrDefault(a => a.Abbreviation == affiliation);
                if (aff == null) return new List<Team>();
                affId = aff.Id;
            }

            var cond = Expr
                .Of<Xylab.Contesting.Entities.Team>(t => t.Status == 1)
                .CombineIf(category.HasValue, t => t.CategoryId == category)
                .CombineIf(ids != null && ids.Length > 0, t => ids.Contains(t.TeamId))
                .CombineIf(affId.HasValue, t => t.AffiliationId == affId);

            var teams = await Context.ListTeamsAsync(cond);

            // In most situations, the team category is public.
            // So we can filter it in memory.
            return teams
                .WhereIf(@public, t => cats[t.CategoryId].IsPublic)
                .Select(t => new Team(t, affs[t.AffiliationId]))
                .ToList();
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
            var aff = await Context.FindAffiliationAsync(team.AffiliationId);
            return new Team(team, aff);
        }
    }
}
