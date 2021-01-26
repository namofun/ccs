using Ccs.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Components.ParticipantDashboard
{
    public class ParticipantDashboardViewComponent : ViewComponent
    {
        private readonly IContestRepository _service;

        public ParticipantDashboardViewComponent(IContestRepository service)
            => _service = service;

        public async Task<IViewComponentResult> InvokeAsync()
            => View("Default", await _service.FindParticipantOfAsync((ViewData.Model as IUser)?.Id ?? 0));
    }
}
