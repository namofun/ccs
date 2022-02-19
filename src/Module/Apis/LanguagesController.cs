using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for languages to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class LanguagesController : ApiControllerBase<IContestContext>
    {
        /// <summary>
        /// Get all the languages for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <response code="200">Returns all the languages for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Language[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] string[] ids = null)
        {
            return (await Context.ListLanguagesAsync())
                .WhereIf(ids != null && ids.Length > 0, l => ids.Contains(l.Id))
                .Select(l => new Language(l))
                .ToArray();
        }


        /// <summary>
        /// Get the given language for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given language for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Language>> GetOne(
            [FromRoute] int cid,
            [FromRoute] string id)
        {
            var ll = await Context.FindLanguageAsync(id);
            return ll == null ? null : new Language(ll);
        }
    }
}
