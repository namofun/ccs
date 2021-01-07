using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for problems to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class ProblemsController : ApiControllerBase
    {
        /// <summary>
        /// Get all the problems for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <response code="200">Returns all the problems for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Problem[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null)
        {
            return (await Context.FetchProblemsAsync())
                .WhereIf(ids != null && ids.Length > 0, cp => ids.Contains(cp.ProblemId))
                .Select(cp => new Problem(cp))
                .ToArray();
        }


        /// <summary>
        /// Get the given problem for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given problem for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Problem>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var probs = await Context.FetchProblemsAsync();
            var prob = probs.FirstOrDefault(cp => cp.ProblemId == id);
            return new Problem(prob);
        }
    }
}
