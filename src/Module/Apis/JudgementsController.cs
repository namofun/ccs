using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using System;
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
    public class JudgementsController : ApiControllerBase
    {
        /// <summary>
        /// Get all the judgements for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="result">Only show judgements with the given result</param>
        /// <param name="submission_id">Only show judgements for the given submission</param>
        /// <response code="200">Returns all the judgements for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Judgement[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] string result = null,
            [FromQuery] int? submission_id = null)
        {
            var store = Context.GetRequiredService<IJudgingStore>();
            var r2 = JudgementType.For(result);
            var cond = Expr
                .Create<Judging>(j => j.StartTime != null)
                .CombineIf(ids != null && ids.Length > 0, j => ids.Contains(j.Id))
                .CombineIf(submission_id.HasValue, j => j.SubmissionId == submission_id)
                .CombineIf(r2 != Verdict.Unknown, j => j.Status == r2);

            var js = await store.ListAsync(cond, 100000);
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return js.Select(judging => new Judgement(judging, contestTime)).ToArray();
        }


        /// <summary>
        /// Get the given judgement for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given judgement for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Judgement>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var store = Context.GetRequiredService<IJudgingStore>();
            var (j, _, cid2, _, _) = await store.FindAsync(id);
            if (j == null || cid2 != cid || j.StartTime == null) return null;
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return new Judgement(j, contestTime);
        }
    }
}
