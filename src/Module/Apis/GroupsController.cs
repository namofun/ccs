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
    /// The endpoints for groups to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class GroupsController : ApiControllerBase
    {
        /// <summary>
        /// Get all the groups for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="public">Only show public groups, even for users with more permissions</param>
        /// <response code="200">Returns all the groups for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Group[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] bool @public = false)
        {
            var cats = await Context.FetchCategoriesAsync();
            var results = cats.Values;
            if (@public) results = results.Where(c => c.IsPublic);
            if (ids != null && ids.Length > 0) results = results.Where(c => ids.Contains(c.Id));
            return results.Select(c => new Group(c)).ToArray();
        }


        /// <summary>
        /// Get the given group for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given group for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var cats = await Context.FetchCategoriesAsync();
            var cat = cats.GetValueOrDefault(id);
            return cat == null ? null : new Group(cat);
        }
    }
}
