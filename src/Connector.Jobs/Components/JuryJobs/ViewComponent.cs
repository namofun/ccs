using Microsoft.AspNetCore.Mvc;

namespace Xylab.Contesting.Connector.Jobs.Components.JuryJobs
{
    public class JuryJobsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View("Default");
    }
}
