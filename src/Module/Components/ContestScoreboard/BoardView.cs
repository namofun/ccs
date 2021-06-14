using Ccs.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Encodings.Web;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class BoardView : IHtmlContent
    {
        private const int PagingSize = Ccs.CcsDefaults.DefaultScoreboardPagingSize;
        private readonly BoardViewModel _model;
        private readonly bool _usefoot, _inJury;
        private readonly IUrlHelper _urlHelper;
        private readonly int? _page;

        public BoardView(BoardViewModel model, bool useFoot, bool inJury, IUrlHelper urlHelper, int? page)
        {
            _model = model;
            _usefoot = useFoot;
            _inJury = inJury;
            _urlHelper = urlHelper;
            _page = page;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var showCategory = new System.Collections.Generic.HashSet<(string, string)>();
            writer.WriteLine("<table class=\"scoreboard center\">");
            writer.WriteLine("<colgroup><col id=\"scorerank\"/><col/><col id=\"scorelogos\"/><col id=\"scoreteamname\"/></colgroup>");
            writer.WriteLine("<colgroup><col id=\"scoresolv\"/><col id=\"scoretotal\"/></colgroup>");

            writer.Write("<colgroup>");
            for (int i = _model.Problems.Count; i > 0; i--)
                writer.Write("<col class=\"scoreprob\"/>");
            writer.Write("</colgroup>");

            writer.WriteLine("<thead><tr class=\"scoreheader\">");
            writer.WriteLine("<th title=\"rank\" scope=\"col\">rank</th>");
            writer.WriteLine("<th title=\"team name\" scope=\"col\" colspan=\"3\">team</th>");
            writer.WriteLine("<th title=\"# solved / penalty time\" colspan=\"2\" scope=\"col\">score</th>");

            foreach (var prob in _model.Problems)
            {
                writer.Write("<th title=\"problem ");
                writer.Write(prob.Title);
                writer.Write("\" scope=\"col\">");

                if (_inJury)
                {
                    writer.Write("<a href=\"");
                    writer.Write(_urlHelper.Action("Detail", "JuryProblems", new { probid = prob.ProblemId }));
                    writer.Write("\">");
                }
                else
                {
                    writer.Write("<a>");
                }

                writer.Write(prob.ShortName);
                writer.Write(" <div class=\"circle\" style=\"background:");
                writer.Write(prob.Color);
                writer.Write(";\"></div></a></th>");
            }

            writer.WriteLine("</tr></thead>");
            int firstRow = 0, lastRow = int.MaxValue, maxRow = 0;
            if (_page.HasValue)
            {
                // display: [first, last)
                firstRow = (_page.Value - 1) * PagingSize;
                lastRow = firstRow + PagingSize;
            }

            foreach (var sortOrder in _model)
            {
                if (sortOrder.Count < firstRow) continue;
                if (sortOrder.Count > maxRow) maxRow = sortOrder.Count;
                writer.WriteLine("<tbody>");

                for (int i = firstRow; i < lastRow && i < sortOrder.Count; i++)
                {
                    var team = sortOrder[i];
                    TeamRow.WriteTo(team, writer, encoder, _inJury, _urlHelper);
                    if (team.Category != null)
                        showCategory.Add((team.CategoryColor, team.Category));
                }

                if (sortOrder.Statistics != null)
                    Statistics.WriteTo(sortOrder.Statistics, writer, encoder);
                writer.WriteLine("</tbody>");
            }

            writer.Write("</table><style>");

            foreach (var (color, _) in showCategory)
            {
                writer.Write(".cl_");
                writer.Write(color[1..]);
                writer.Write("{background-color:");
                writer.Write(color);
                writer.Write(";}");
            }

            writer.WriteLine("</style>");

            if (_page.HasValue)
            {
                int page = _page ?? 7;
                int minimalPage = 1, maximumPage = (maxRow - 1) / PagingSize + 1;
                int l = Math.Max(minimalPage, page - 3);
                int r = Math.Min(page + 3, maximumPage);

                void WritePageLink(string page, bool disabled = false, bool current = false)
                    => writer.Write(disabled
                        ? "<li class=\"page-item disabled\"><a class=\"page-link\" href=\"#\" tabindex=\"-1\" aria-disabled=\"true\">" + page + "</a></li>"
                        : "<li class=\"page-item" + (current ? " active\" aria-current=\"page" : "") + "\"><a class=\"page-link\" href=\"#\">" + page + "</a></li>");

                writer.Write("<ul class=\"pagination justify-content-center mt-3 mb-3 dom-pagination\">");
                WritePageLink("&laquo;");
                WritePageLink("&lsaquo;");
                for (int i = l; i <= r; i++)
                    WritePageLink(i.ToString(), current: i == page);
                WritePageLink("&rsaquo;");
                WritePageLink("&raquo;");
                writer.Write("</ul>");
            }

            if (_usefoot)
                Footer.WriteTo(showCategory, writer, encoder, _model.RankingStrategy, !_page.HasValue);
        }
    }
}
