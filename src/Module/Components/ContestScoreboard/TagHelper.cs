using Ccs.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    /// <summary>
    /// Returning an <see cref="IHtmlContent"/> representing the domjudge-style scoreboard page.
    /// </summary>
    /// <remarks>This component is directly written in C# because the feature option enabling is hard in razor syntax.</remarks>
    [HtmlTargetElement("domjudge-scoreboard", TagStructure = TagStructure.WithoutEndTag)]
    public class ScoreboardTagHelper : XysTagHelper
    {
        [HtmlAttributeName("model")]
        public BoardViewModel Model { get; set; }

        [HtmlAttributeName("use-footer")]
        public bool UseFooter { get; set; }

        [HtmlAttributeName("in-jury")]
        public bool InJury { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagName = null;
            output.Content.SetHtmlContent(new BoardView(Model, UseFooter, InJury));
        }
    }
}
