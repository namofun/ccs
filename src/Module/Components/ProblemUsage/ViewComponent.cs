﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xylab.Contesting.Services;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Components.ProblemUsage
{
    public class ProblemUsageViewComponent : ViewComponent
    {
        private readonly IContestRepository _service;

        public ProblemUsageViewComponent(IContestRepository service)
            => _service = service;

        public async Task<IViewComponentResult> InvokeAsync()
            => View("Default", await _service.FindProblemUsageAsync((ViewData.Model as Problem)?.Id ?? 0));
    }
}
