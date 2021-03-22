using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Polygon.Storages;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Components.ContestStatistics
{
    public class ContestStatisticsViewComponent : ViewComponent
    {
        private readonly ISubmissionStore _service;
        private readonly ContestStatisticsOptions _options;

        public ContestStatisticsViewComponent(
            ISubmissionStore service,
            IOptions<ContestStatisticsOptions> options)
        {
            _options = options.Value;
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_options.DefaultContest.HasValue
                || !int.TryParse(UserClaimsPrincipal.GetUserId(), out int userId))
            {
                return Content("");
            }
            else
            {
                return View("Default",
                    await _service.StatisticsAsync(
                        _options.DefaultContest.Value,
                        userId));
            }
        }
    }
}
