using System;
using System.Collections.Generic;

namespace Ccs.Models
{
    public class ProblemStatisticsModel
    {
        public int Accepted { get; set; }

        public int Rejected { get; set; }

        public int Pending { get; set; }

        public int? FirstSolve { get; set; }

        public int MaxScore { get; set; }

        public IReadOnlyList<(string Icon, string Title, string Value)> GetStatistics(int rule)
        {
            if (rule == CcsDefaults.RuleXCPC)
            {
                return new[]
                {
                    ("thumbs-up", "number of accepted submissions", $"{Accepted}"),
                    ("thumbs-down", "number of rejected submissions", $"{Rejected}"),
                    ("question-circle", "number of pending submissions", $"{Pending}"),
                    ("clock", "first solved", FirstSolve.HasValue ? FirstSolve.Value + "min" : "n/a"),
                };
            }
            else if (rule == CcsDefaults.RuleIOI)
            {
                return new[]
                {
                    ("user", "number of teams submitted", $"{Accepted}"),
                    ("hockey-puck", "number of submissions", $"{Accepted + Rejected}"),
                    ("question-circle", "number of pending submissions", $"{Pending}"),
                    ("lightbulb", "current maximum score", $"{MaxScore}pts"),
                };
            }
            else
            {
                return Array.Empty<(string, string, string)>();
            }
        }
    }
}
