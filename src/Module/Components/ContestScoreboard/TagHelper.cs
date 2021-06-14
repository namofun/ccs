using Ccs.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

        [HtmlAttributeName("page")]
        public int? Page { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IUrlHelperFactory UrlHelperFactory { get; }

        public ScoreboardTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            UrlHelperFactory = urlHelperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagName = null;
            var urlHelper = UrlHelperFactory.GetUrlHelper(ViewContext);
            output.Content.SetHtmlContent(new BoardView(Model, UseFooter, InJury, urlHelper, Page));
        }
    }
}
