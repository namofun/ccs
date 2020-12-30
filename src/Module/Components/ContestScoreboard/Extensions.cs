using Ccs.Models;
using Microsoft.AspNetCore.Html;
using SatelliteSite.ContestModule.Components.ContestScoreboard;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class ScoreboardHtmlHelperExtensions
    {
        /// <summary>
        /// Creates an <see cref="IHtmlContent"/> representing the contest-style scoreboard page.
        /// </summary>
        /// <param name="_">The <see cref="IHtmlHelper"/>.</param>
        /// <param name="model">The scoreboard view model.</param>
        /// <param name="useFooter">Whether to output a footer.</param>
        /// <param name="inJury">Whether to activate the jury features.</param>
        /// <returns>The board view content.</returns>
        /// <remarks>This component is directly written in C# because the feature option enabling is hard in razor syntax.</remarks>
        public static IHtmlContent ContestScoreboard(this IHtmlHelper _, BoardViewModel model, bool useFooter, bool inJury)
        {
            return new BoardView(model, useFooter, inJury);
        }
    }
}
