using Ccs.Models;
using Microsoft.AspNetCore.Html;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class Statistics : IHtmlContent
    {
        private readonly ProblemStatisticsModel[] _model;
        public Statistics(ProblemStatisticsModel[] model) => _model = model;
        public void WriteTo(TextWriter writer, HtmlEncoder encoder) => WriteTo(_model, writer, encoder);

        public static void WriteTo(ProblemStatisticsModel[] model, TextWriter writer, HtmlEncoder encoder, int rule = 0)
        {
            writer.Write("<tr class=\"sortorder_summary\">");
            writer.Write("<td id=\"scoresummary\" title=\"Summary\" colspan=\"4\">Summary</td>");
            writer.Write("<td title=\"total solved\" class=\"scorenc\">");

            writer.Write(rule switch
            {
                Ccs.CcsDefaults.RuleXCPC => model.Sum(r => r.Accepted),
                Ccs.CcsDefaults.RuleIOI => model.Sum(r => r.MaxScore),
                _ => model.Sum(r => r.Accepted),
            });

            writer.Write("</td><td></td>");

            foreach (var item in model)
            {
                var stats = item.GetStatistics(rule);
                writer.Write("<td style=\"text-align: left;\"><a>");

                for (int i = 0; i < stats.Count; i++)
                {
                    if (i != 0) writer.Write("<br/>");
                    writer.Write("<i class=\"fas fa-");
                    writer.Write(stats[i].Icon);
                    writer.Write(" fa-fw\"></i><span style=\"font-size:90%;\" title=\"");
                    writer.Write(stats[i].Title);
                    writer.Write("\">");
                    writer.Write(stats[i].Value);
                    writer.Write("</span>");
                }

                writer.Write("</a></td>");
            }

            writer.WriteLine("</tr>");
        }
    }
}
