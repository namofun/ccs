﻿using Ccs.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints for judgement-types to connect to CDS.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/[controller]")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "CDS,Administrator")]
    [Produces("application/json")]
    public class JudgementTypesController : ApiControllerBase
    {
        /// <summary>
        /// Get all the judgement types for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="ids">Filter the objects to get on this list of ID's</param>
        /// <response code="200">Returns all the judgement types for this contest</response>
        [HttpGet]
        public ActionResult<JudgementType[]> GetAll(
            [FromRoute] int cid,
            [FromQuery] string[] ids = null)
        {
            if (ids.Length == 0)
                return JudgementType.Defaults;
            return ids
                .Join(JudgementType.Defaults, s => s, j => j.Id, (s, j) => j)
                .ToArray();
        }


        /// <summary>
        /// Get the given judgement type for this contest
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="id">The ID of the entity to get</param>
        /// <response code="200">Returns the given judgement type for this contest</response>
        [HttpGet("{id}")]
        public ActionResult<JudgementType> GetOne(
            [FromRoute] int cid,
            [FromRoute] string id)
        {
            return JudgementType.Defaults.FirstOrDefault(j => j.Id == id);
        }
    }
}