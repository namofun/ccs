using Microsoft.AspNetCore.Mvc;

namespace Xylab.Contesting.Connector.OpenXml.Components.JuryOpenXmlScoreboard
{
    public class JuryOpenXmlScoreboardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View("Default");
    }
}
