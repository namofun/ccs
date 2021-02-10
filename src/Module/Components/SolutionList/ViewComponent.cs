using Microsoft.AspNetCore.Mvc;
using Polygon.Models;
using System.Collections.Generic;

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
