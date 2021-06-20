using Ccs.Models;
using Ccs.Scoreboard;
using Microsoft.AspNetCore.Html;
using System.IO;
using System.Text.Encodings.Web;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class Statistics : IHtmlContent
    {
        private readonly ProblemStatisticsModel[] _model;
        private readonly IRankingStrategyV2 _rule;
        private readonly ProblemCollection _problems;
        private readonly IContestTime _time;
        
        public Statistics(ProblemStatisticsModel[] model, IRankingStrategyV2 rule, ProblemCollection problems, IContestTime time)
            => (_model, _rule, _problems, _time) = (model, rule, problems, time);

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            => WriteTo(_model, _rule, _problems, _time, writer, encoder);

        public static void WriteTo(ProblemStatisticsModel[] model, IRankingStrategyV2 rule, ProblemCollection problems, IContestTime time, TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<tr class=\"sortorder_summary\">");
            writer.Write("<td id=\"scoresummary\" title=\"Summary\" colspan=\"4\">Summary</td>");
            writer.Write("<td title=\"total solved\" class=\"scorenc\">");
            writer.Write(rule.GetTotalSolved(model));
            writer.Write("</td><td></td>");

            for (int j = 0; j < model.Length; j++)
            {
                var stats = rule.GetStatistics(model[j], problems[j], time);
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
