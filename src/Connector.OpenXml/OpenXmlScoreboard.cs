using Ccs.Models;
using ClosedXML.Excel;
using System;
using System.Linq;

namespace Ccs.Scoreboard
{
    internal static class OpenXmlScoreboard
    {
        public const string MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static XLWorkbook Create(FullBoardViewModel board, string contestName)
        {
            var workbook = new XLWorkbook(XLEventTracking.Disabled);

            foreach (var sortOrder in board)
            {
                var teams = sortOrder.ToList();
                var cats = string.Join(", ", teams.Select(t => t.Category).Distinct().Where(a => !string.IsNullOrWhiteSpace(a)));
                var worksheet = workbook.Worksheets.Add(cats);

                worksheet.Range(1, 1, 1, 4 + board.Problems.Count).Merge().Value(contestName).Bold().Align();
                worksheet.Range(2, 1, 2, 4 + board.Problems.Count).Merge().Value($"导出时间：{DateTimeOffset.Now}").Align(XLAlignmentHorizontalValues.Right);

                worksheet.Row(1).Height *= 3;
                worksheet.Row(1).Style.Font.FontSize *= 1.5;
                worksheet.Row(2).Height *= 1.5;
                worksheet.Row(3).Height *= 1.5;
                worksheet.Column(2).Width *= 2;

                worksheet.Cell(3, 1).Value("RANK").Bold().Align();
                worksheet.Cell(3, 2).Value("TEAM").Bold().Align();
                worksheet.Range(3, 3, 3, 4).Merge().Value("SCORE").Bold().Align();
                for (int i = 0; i < board.Problems.Count; i++) worksheet.Cell(3, 5 + i).Value(board.Problems[i].ShortName).Bold().Align();

                int rowid = 4;
                foreach (var team in teams)
                {
                    var row = worksheet.Row(rowid++);
                    row.Height *= 1.5;

                    row.Cell(1).Value(team.Rank).Align();
                    row.Cell(2).Value(team.TeamName).Bold().Bgcolor(XLColor.FromHtml(team.CategoryColor)).Align(XLAlignmentHorizontalValues.Right);
                    row.Cell(3).Value(team.Points).Bold().Align();
                    row.Cell(4).Value(team.Penalty).Align();

                    for (int i = 0; i < team.Problems.Length; i++)
                    {
                        if (board.RankingStrategy.Id == CcsDefaults.RuleXCPC)
                        {
                            if (team.Problems[i] == null) continue;
                            var prob = team.Problems[i];
                            if (!prob.Score.HasValue && prob.JudgedCount == 0) continue;

                            row.Cell(5 + i).Align()
                                .Value(!prob.Score.HasValue
                                    ? $"(-{prob.JudgedCount})"
                                    : $"{prob.Score} (+{prob.JudgedCount})");
                        }
                        else if (board.RankingStrategy.Id == CcsDefaults.RuleIOI)
                        {
                            if (team.Problems[i]?.Score is not int sc) continue;
                            row.Cell(5 + i).Align().Value(sc);
                        }
                    }
                }
            }

            // sentinel worksheet
            workbook.AddWorksheet("Sheet0");
            return workbook;
        }
    }
}
