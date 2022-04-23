using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Services;
using Xylab.Contesting.Specifications;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for judgements to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class JudgementsController : ApiControllerBase<ISubmissionContext>
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
            var r2 = JudgementType.For(result);
            var cond = Expr
                .Of<Judging>(j => j.StartTime != null && j.s.ContestId == cid)
                .CombineIf(ids != null && ids.Length > 0, j => ids.Contains(j.Id))
                .CombineIf(submission_id.HasValue, j => j.SubmissionId == submission_id)
                .CombineIf(r2 != Verdict.Unknown, j => j.Status == r2);

            var js = await Context.ListJudgingsAsync(cond, 100000);
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
            var j = await Context.FindJudgingAsync(id);
            if (j == null || j.StartTime == null) return null;
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return new Judgement(j, contestTime);
        }
    }
}
