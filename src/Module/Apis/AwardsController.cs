using Ccs.Services;
using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for awards to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class AwardsController : ApiControllerBase<IContestContext>
    {
        /// <summary>
        /// Get all the awards standings for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="strict">Whether to only include CCS compliant properties in the response</param>
        /// <response code="200">Returns the current teams qualifying for each award</response>
        [HttpGet]
        public ActionResult<Award[]> GetAll(
            [FromRoute] int cid,
            [FromQuery] bool strict = false)
        {
            // There's no design for this module.
            return Array.Empty<Award>();
        }


        /// <summary>
        /// Get the given award for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="strict">Whether to only include CCS compliant properties in the response</param>
        /// <response code="200">Returns the award for this contest</response>
        [HttpGet("{id}")]
        public ActionResult<Award> GetOne(
            [FromRoute] int cid,
            [FromRoute] int id,
            [FromQuery] bool strict = false)
        {
            // There's no design for this module.
            return null;
        }
    }
}
