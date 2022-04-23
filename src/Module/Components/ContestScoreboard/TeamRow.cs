using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Encodings.Web;
using Xylab.Contesting.Models;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class TeamRow : IHtmlContent
    {
        private readonly TeamModel _model;
        private readonly bool _inJury;
        private readonly IUrlHelper _urlHelper;

        public TeamRow(TeamModel model, bool inJury, IUrlHelper urlHelper) => (_model, _inJury, _urlHelper) = (model, inJury, urlHelper);

        public void WriteTo(TextWriter writer, HtmlEncoder encoder) => WriteTo(_model, writer, encoder, _inJury, _urlHelper);

        public static void WriteTo(TeamModel model, TextWriter writer, HtmlEncoder encoder, bool inJury, IUrlHelper urlHelper, string styleClass = "")
        {
            writer.Write("<tr class=\"");
            writer.Write(styleClass);
            writer.Write("\" id=\"team:");
            writer.Write(model.TeamId);
            writer.Write("\">");

            writer.Write("<td class=\"scorepl\">");
            if (!model.Rank.HasValue)
                writer.Write("?");
            else if (model.ShowRank)
                writer.Write(model.Rank.Value);
            writer.Write("</td>");

            writer.Write("<td class=\"scoreaf\"></td>");

            writer.Write("<td class=\"scoreaf\">");
            if (model.AffiliationId != "null")
            {
                writer.Write("<a><img src=\"/images/affiliations/");
                writer.Write(model.AffiliationId);
                writer.Write(".png\" alt=\"\" title=\"");
                writer.Write(encoder.Encode(model.Affiliation));
                writer.Write("\" class=\"affiliation-logo\" /></a>");
            }
            writer.Write("</td>");

            writer.Write("<td class=\"scoretn cl_");
            writer.Write(model.CategoryColor?.Substring(1) ?? "");
            writer.Write("\" title=\"");
            var teamName = encoder.Encode(model.TeamName);
            writer.Write(teamName);
            writer.Write("\"><a");

            if (inJury)
            {
                writer.Write(" href=\"");
                writer.Write(urlHelper.Action("Detail", "JuryTeams", new { teamid = model.TeamId }));
                writer.Write("\"");
            }

            writer.Write("><span class=\"forceWidth\">");
            writer.Write(teamName);
            writer.Write("</span><span class=\"univ forceWidth\">");
            if (model.AffiliationId != "null")
                writer.Write(encoder.Encode(model.Affiliation));
            writer.Write("</span></a></td>");

            writer.Write("<td class=\"scorenc\">");
            writer.Write(model.Points);
            writer.Write("</td><td class=\"scorett\">");
            writer.Write(model.Penalty);
            writer.Write("</td>");

            foreach (var item in model.Problems)
            {
                writer.Write("<td class=\"score_cell\">");

                if (item != null && item.PendingCount + item.JudgedCount > 0)
                {
                    writer.Write("<a><div class=\"");
                    writer.Write(item.StyleClass);
                    writer.Write("\">");
                    if (item.Score.HasValue)
                        writer.Write(item.Score.Value);
                    else
                        writer.Write("&nbsp;");
                    writer.Write("<span>");
                    writer.Write(item.Tries);
                    writer.Write("</span></div></a>");
                }

                writer.Write("</td>");
            }

            writer.WriteLine("</tr>");
        }
    }
}
