using Ccs.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var showCategory = new HashSet<(string, string)>();
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

            void WriteTeamRow(TeamModel team, bool showRank)
            {
                team.ShowRank |= showRank;
                TeamRow.WriteTo(team, writer, encoder, _inJury, _urlHelper);
                if (team.Category != null)
                    showCategory.Add((team.CategoryColor, team.Category));
            }

            var ambient = new List<TeamModel>();

            foreach (var sortOrder in _model)
            {
                HashSet<int> favoriteTeams = null;
                if (_page.HasValue) favoriteTeams = (_model as FullBoardViewModel)?.FavoriteTeams;
                if (favoriteTeams?.Count == 0) favoriteTeams = null;

                if (sortOrder.Count < firstRow && (favoriteTeams == null || !sortOrder.Any(t => favoriteTeams.Contains(t.TeamId)))) continue;
                if (sortOrder.Count > maxRow) maxRow = sortOrder.Count;

                writer.WriteLine("<tbody>");
                for (int i = firstRow; i < lastRow && i < sortOrder.Count; i++)
                    WriteTeamRow(sortOrder[i], i == firstRow);
                if (sortOrder.Statistics != null && sortOrder.Count >= firstRow)
                    Statistics.WriteTo(sortOrder.Statistics, _model.RankingStrategy, _model.Problems, _model.ContestTime, writer, encoder);
                writer.WriteLine("</tbody>");

                for (int i = 0; favoriteTeams != null && i < sortOrder.Count; i++)
                    if ((i < firstRow || i >= lastRow) && favoriteTeams.Contains(sortOrder[i].TeamId))
                        ambient.Add(sortOrder[i]);
            }

            if (ambient.Count > 0)
            {
                writer.WriteLine("<tbody class=\"d-none\" id=\"ambient-favorite-teams\">");
                foreach (var item in ambient) WriteTeamRow(item, true);
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

            if (_model is FullBoardViewModel fbvm && _page.HasValue)
            {
                int page = _page.Value;
                int minimalPage = 1, maximumPage = (maxRow - 1) / PagingSize + 1;
                int l = Math.Max(minimalPage, page - 3);
                int r = Math.Min(page + 3, maximumPage);

                var routeArgs = new Dictionary<string, object>();
                if (fbvm.FilteredAffiliations != null) routeArgs["affiliations[]"] = fbvm.FilteredAffiliations;
                if (fbvm.FilteredCategories != null) routeArgs["categories[]"] = fbvm.FilteredCategories;
                if (routeArgs.Count > 0) routeArgs["filter"] = "filter";

                void WritePageLink(string pageString, int realPage, bool disabled = false, bool current = false)
                    => writer.Write(disabled
                        ? "<li class=\"page-item disabled\"><a class=\"page-link\" href=\"#\" tabindex=\"-1\" aria-disabled=\"true\">" + pageString + "</a></li>"
                        : "<li class=\"page-item" + (current ? " active\" aria-current=\"page" : "") + "\"><a class=\"page-link\" href=\"" + _urlHelper.Action(null, new Dictionary<string, object>(routeArgs) { ["page"] = realPage }) + "\">" + pageString + "</a></li>");

                writer.Write("<ul class=\"pagination justify-content-center mt-3 mb-3 dom-pagination\">");
                WritePageLink("&laquo;", minimalPage, disabled: page == 1);
                WritePageLink("&lsaquo;", page - 1, disabled: page == 1);
                for (int i = l; i <= r; i++)
                    WritePageLink(i.ToString(), i, current: i == page);
                WritePageLink("&rsaquo;", page + 1, disabled: page == maximumPage);
                WritePageLink("&raquo;", maximumPage, disabled: page == maximumPage);
                writer.Write("</ul>");
            }

            if (_usefoot)
                Footer.WriteTo(showCategory, _model.RankingStrategy, writer, encoder, !_page.HasValue);
        }
    }
}
