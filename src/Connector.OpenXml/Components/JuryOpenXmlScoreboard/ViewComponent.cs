using Microsoft.AspNetCore.Mvc;

namespace Ccs.Connector.OpenXml.Components.JuryOpenXmlScoreboard
{
    public class JuryOpenXmlScoreboardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View("Default");
    }
}
