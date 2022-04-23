using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Xylab.Contesting.Scoreboard;

namespace SatelliteSite.ContestModule.Components.ContestScoreboard
{
    public class Footer : IHtmlContent
    {
        private readonly HashSet<(string, string)> _cats;
        private readonly IRankingStrategyV2 _rs;

        public Footer(HashSet<(string, string)> cats, IRankingStrategyV2 rs)
            => (_cats, _rs) = (cats, rs);

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            => WriteTo(_cats, _rs, writer, encoder);

        public static void WriteTo(HashSet<(string, string)> cats, IRankingStrategyV2 rs, TextWriter writer, HtmlEncoder encoder, bool leavePre = true)
        {
            if (leavePre) writer.WriteLine("<p><br /><br /></p>");
            writer.WriteLine("<table id=\"cell_legend\" class=\"scoreboard scorelegend\">");
            writer.WriteLine("<thead><tr><th scope=\"col\">Cell colours</th></tr></thead><tbody>");
            foreach (var (@class, name) in rs.CellColors)
                writer.WriteLine($"<tr class=\"{@class}\"><td>{name}</td></tr>");
            writer.WriteLine("</tbody></table>");

            if (cats.Count > 1)
            {
                writer.WriteLine("<table id=\"categ_legend\" class=\"scoreboard scorelegend\">");
                writer.WriteLine("<thead><tr><th scope=\"col\"><a>Categories</a></th></tr></thead><tbody>");

                foreach (var item in cats)
                {
                    writer.Write("<tr style=\"background: ");
                    writer.Write(item.Item1);
                    writer.Write(";\"><td><a>");
                    writer.Write(encoder.Encode(item.Item2));
                    writer.WriteLine("</a></td></tr>");
                }

                writer.WriteLine("</tbody></table>");
            }
        }
    }
}
