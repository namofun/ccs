using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for runs to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class RunsController : ApiControllerBase<ISubmissionContext>
    {
        /// <summary>
        /// Get all the runs for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="first_id">Only show runs starting from this ID</param>
        /// <param name="last_id">Only show runs until this ID</param>
        /// <param name="judging_id">Only show runs for this judgement</param>
        /// <param name="limit">Limit the number of returned runs to this amount</param>
        /// <response code="200">Returns all the runs for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Run[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] int? first_id = null,
            [FromQuery] int? last_id = null,
            [FromQuery] int? judging_id = null,
            [FromQuery] int? limit = null)
        {
            var condition = Expr
                .Of<Testcase, JudgingRun>((t, d) => d.j.s.ContestId == cid)
                .CombineIf(ids != null && ids.Length > 0, (t, d) => ids.Contains(d.Id))
                .CombineIf(first_id.HasValue, (t, d) => d.Id >= first_id)
                .CombineIf(last_id.HasValue, (t, d) => d.Id <= last_id)
                .CombineIf(judging_id.HasValue, (t, d) => d.JudgingId == judging_id);

            var runs = await Context.GetDetailsAsync(
                selector: (t, d) => new { d.JudgingId, d.CompleteTime, d.ExecuteTime, d.Status, d.Id, t.Rank },
                predicate: condition, limit: limit);

            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return runs
                .Select(run => new Run(run.CompleteTime, run.CompleteTime - contestTime, run.Id, run.JudgingId, run.Status, run.Rank, run.ExecuteTime))
                .ToArray();
        }


        /// <summary>
        /// Get the given run for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given run for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Run>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var runQuery = await Context.GetDetailsAsync(
                selector: (t, d) => new { d.JudgingId, d.CompleteTime, d.ExecuteTime, d.Status, d.Id, t.Rank },
                predicate: (t, d) => d.j.s.ContestId == cid && d.Id == id);
            var run = runQuery.SingleOrDefault();

            if (run == null) return null;
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return new Run(run.CompleteTime, run.CompleteTime - contestTime, run.Id, run.JudgingId, run.Status, run.Rank, run.ExecuteTime);
        }
    }
}
