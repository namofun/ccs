using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Services;
using Xylab.Contesting.Specifications;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for submissions to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class SubmissionsController : ApiControllerBase<ISubmissionContext>
    {
        /// <summary>
        /// Get all the submissions for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <param name="language_id">Only show submissions for the given language</param>
        /// <response code="200">Returns all the submissions for this contest</response>
        [HttpGet]
        public async Task<ActionResult<Submission[]>> GetAll(
            [FromRoute] int cid,
            [FromQuery] int[] ids = null,
            [FromQuery] string language_id = null)
        {
            var condition = Expr
                .Of<Xylab.Polygon.Entities.Submission>(s => s.ContestId == cid)
                .CombineIf(ids != null && ids.Length > 0, s => ids.Contains(s.Id))
                .CombineIf(language_id != null, s => s.Language == language_id);

            var submissions = await Context.ListSubmissionsAsync(
                predicate: condition,
                projection: s => new { s.Language, s.Id, s.ProblemId, s.TeamId, s.Time });
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return submissions
                .Select(s => new Submission(cid, s.Language, s.Id, s.ProblemId, s.TeamId, s.Time, s.Time - contestTime))
                .ToArray();
        }


        /// <summary>
        /// Get the given submission for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given submission for this contest</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Submission>> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id)
        {
            var ss = await Context.FindSubmissionAsync(id);
            if (ss == null) return null;
            var contestTime = Contest.StartTime ?? DateTimeOffset.Now;
            return new Submission(cid, ss.Language, ss.Id, ss.ProblemId, ss.TeamId, ss.Time, ss.Time - contestTime);
        }


        /// <summary>
        /// Restore a submission for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="code">The source code without BASE64 encode</param>
        /// <param name="ip">The source IP address</param>
        /// <param name="langid">The language id</param>
        /// <param name="probid">The problem id</param>
        /// <param name="teamid">The team id</param>
        /// <param name="time">The submission time, or now if not specified</param>
        /// <response code="201">Added</response>
        [HttpPost("[action]")]
        [ProducesResponseType(201, Type = typeof(Submission))]
        public async Task<ActionResult<Submission>> Restore(
            [FromRoute] int cid,
            [FromForm, Required] int probid,
            [FromForm, Required] int teamid,
            [FromForm, Required] string langid,
            [FromForm, Required] string code,
            [FromForm, Required] string ip,
            [FromForm] DateTimeOffset? time)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            var lang = await Context.FindLanguageAsync(langid, contestFiltered: false);
            var prob = await Context.FindProblemAsync(probid);
            if (team == null || lang == null || prob == null) return BadRequest();

            var s = await Context.SubmitAsync(
                code: code,
                language: lang,
                problem: prob,
                team: team,
                ipAddr: System.Net.IPAddress.Parse(ip),
                via: "restorer",
                username: "api",
                time: time);

            return CreatedAtAction(nameof(GetOne), new { cid, id = s.Id },
                new Submission(cid, s.Language, s.Id, s.ProblemId, s.TeamId, s.Time, s.Time - (Contest.StartTime ?? DateTimeOffset.Now)));
        }
    }
}
