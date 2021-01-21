using Ccs.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Encodings.Web;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class BoardView : IHtmlContent
    {
        private readonly BoardViewModel _model;
        private readonly bool _usefoot, _inJury;
        private readonly IUrlHelper _urlHelper;

        public BoardView(BoardViewModel model, bool useFoot, bool inJury, IUrlHelper urlHelper)
        {
            _model = model;
            _usefoot = useFoot;
            _inJury = inJury;
            _urlHelper = urlHelper;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
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

            foreach (var sortOrder in _model)
            {
                int totalPoints = 0;
                writer.WriteLine("<tbody>");

                foreach (var team in sortOrder)
                {
                    totalPoints += team.Points;
                    TeamRow.WriteTo(team, writer, encoder, _inJury, _urlHelper);
                    if (team.Category != null)
                        _model.ShowCategory.Add((team.CategoryColor, team.Category));
                }

                if (sortOrder.Statistics != null)
                    Statistics.WriteTo(sortOrder.Statistics, totalPoints, writer, encoder);
                writer.WriteLine("</tbody>");
            }

            writer.Write("</table><style>");

            foreach (var (color, _) in _model.ShowCategory)
            {
                writer.Write(".cl_");
                writer.Write(color[1..]);
                writer.Write("{background-color:");
                writer.Write(color);
                writer.Write(";}");
            }

            writer.WriteLine("</style>");

            if (_usefoot)
                Footer.WriteTo(_model.ShowCategory, writer, encoder, _model.RankingStrategy);
        }
    }
}
