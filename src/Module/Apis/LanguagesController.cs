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
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class LanguagesController : ApiControllerBase
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
            var langs = await Context.FetchLanguagesAsync();
            var query = langs.AsEnumerable();
            if (ids != null && ids.Length > 0)
                query = query.Where(l => ids.Contains(l.Id));
            return query.Select(l => new Language(l)).ToArray();
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
            var langs = await Context.FetchLanguagesAsync();
            var ll = langs
                .Where(l => l.Id == id && l.AllowSubmit)
                .FirstOrDefault();
            return ll == null ? null : new Language(ll);
        }
    }
}
