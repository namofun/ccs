﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xylab.Contesting.Services;
using Xylab.Contesting.Specifications;
using Xylab.Polygon.Models;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for contest and state to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class ContestsController : ApiControllerBase<IJuryContext>
    {
        /// <summary>
        /// Get the given contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <response code="200">Returns the given contest</response>
        [HttpGet]
        public ActionResult<Contest> Info(
            [FromRoute] int cid)
        {
            return new Contest(Contest);
        }


        /// <summary>
        /// Change the start time of the given contest
        /// </summary>
        /// <param name="cid">The ID of the contest to change the start time for</param>
        /// <param name="start_time">The new start time of the contest</param>
        /// <response code="200">Contest start time changed successfully</response>
        /// <response code="403">Changing start time not allowed</response>
        [HttpPatch]
        [AuditPoint(AuditlogType.Contest)]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ChangeTime(
            [FromRoute] int cid,
            [FromForm] DateTimeOffset? start_time)
        {
            var now = DateTimeOffset.Now;
            var newTime = start_time ?? (now + TimeSpan.FromSeconds(30));
            var oldtime = Contest.StartTime.Value;

            if (Contest.GetState(now) >= Xylab.Contesting.Entities.ContestState.Started)
                return StatusCode(403); // contest is started
            if (newTime < now + TimeSpan.FromSeconds(29.5))
                return StatusCode(403); // new start time is in the past or within 30s
            if (now + TimeSpan.FromSeconds(30) > oldtime)
                return StatusCode(403); // left time

            await Context.UpdateContestAsync(
                _ => new()
                {
                    StartTime = newTime,
                });

            await HttpContext.AuditAsync("changed time", $"{Contest.Id}", "via ccs-api");
            return Ok();
        }


        /// <summary>
        /// Get the current contest state
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <response code="200">The contest state</response>
        [HttpGet("[action]")]
        public ActionResult<State> State(
            [FromRoute] int cid)
        {
            return new State(Contest);
        }


        /// <summary>
        /// Get the event feed for the given contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="since_id">Only get events after this event</param>
        /// <param name="types">Types to filter the event feed on</param>
        /// <param name="stream">Whether to stream the output or stop immediately</param>
        /// <response code="200">The events</response>
        [HttpGet("[action]")]
        [Produces("application/x-ndjson")]
        public IActionResult EventFeed(
            [FromRoute] int cid,
            [FromQuery] int? since_id = null,
            [FromQuery] string types = null,
            [FromQuery] bool stream = true)
        {
            string[] endpointTypes = string.IsNullOrWhiteSpace(types) ? null : types.Split(',');
            return new EventFeedResult(endpointTypes, stream, since_id ?? 0);
        }


        /// <summary>
        /// Get general status information
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <response code="200">General status information for the given contest</response>
        [HttpGet("[action]")]
        public async Task<ActionResult<ServerStatus>> Status(
            [FromRoute] int cid)
        {
            return await Context.GetJudgeQueueAsync();
        }
    }
}
