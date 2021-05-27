using Microsoft.AspNetCore.Mvc;

namespace Ccs.Connector.Jobs.Components.JuryJobs
{
    public class JuryJobsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View("Default");
    }
}
