using Ccs.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Components.ContestStatistics
{
    public class ContestStatisticsViewComponent : ViewComponent
    {
        private readonly IContestRepository2 _service;
        private readonly ContestStatisticsOptions _options;

        public ContestStatisticsViewComponent(
            IContestRepository2 service,
            IOptions<ContestStatisticsOptions> options)
        {
            _options = options.Value;
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = (IUser)ViewData["User"];
            if (!_options.DefaultContest.HasValue)
            {
                return Content("");
            }
            else
            {
                ViewData["DefaultContestId"] = _options.DefaultContest.Value;
                return View("Default",
                    await _service.StatisticsAsync(
                        _options.DefaultContest.Value,
                        user.Id));
            }
        }
    }
}
