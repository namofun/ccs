using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for organizations to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class OrganizationsController : ApiControllerBase
    {
        /// <summary>
        /// Get all the organizations for this contest
        /// </summary>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="cid">The contest ID</param>
        /// <param name="country">Only show organizations for the given country</param>
        /// <response code="200">Returns all the organizations for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Organization[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] string[] ids = null,
            [FromQuery] string country = null)
        {
            var results = await Context.FetchAffiliationsAsync();
            var query = results.Values;
            if (country != null) query = query.Where(t => t.CountryCode == country);
            if (ids != null && ids.Length > 0) query = query.Where(t => ids.Contains(t.Abbreviation));
            return query.Select(a => new Organization(a)).ToArray();
        }


        /// <summary>
        /// Get the given organization for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given organization for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOne(
            [FromRoute] int cid,
            [FromRoute] string id)
        {
            var results = await Context.FetchAffiliationsAsync();
            var org = results.Values.FirstOrDefault(a => a.Abbreviation == id);
            return org == null ? null : new Organization(org);
        }
    }
}
