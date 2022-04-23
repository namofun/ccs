using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xylab.Polygon.Models;

namespace SatelliteSite.ContestModule.Components.SolutionList
{
    public class SolutionListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IReadOnlyList<Solution> model, bool showTeams, bool showProblems, bool showIp, bool showLanguages)
        {
            return View("Default", new SolutionListViewModel(model)
            {
                ShowIp = showIp,
                ShowProblems = showProblems,
                ShowLanguages = showLanguages,
                ShowTeams = showTeams
            });
        }
    }
}
