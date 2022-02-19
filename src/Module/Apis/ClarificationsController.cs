using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for clarifications to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class ClarificationsController : ApiControllerBase<IClarificationContext>
    {
        /// <summary>
        /// Get all the clarifications for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="problem">Only show clarifications for the given problem</param>
        /// <response code="200">Returns all the clarifications for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Clarification[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] int? problem = null)
        {
            var cond = Expr
                .Of<Ccs.Entities.Clarification>(null)
                .CombineIf(ids != null && ids.Length > 0, c => ids.Contains(c.Id))
                .CombineIf(problem.HasValue, c => c.ProblemId == problem);
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;

            var res = await Context.ListClarificationsAsync(cond);
            return res.Select(c => new Clarification(c, contestTime)).ToArray();
        }


        /// <summary>
        /// Get the given clarifications for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given clarification for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Clarification>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            var clar = await Context.FindClarificationAsync(id);
            return clar == null ? null : new Clarification(clar, contestTime);
        }
    }
}
