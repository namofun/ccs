using Ccs;
using Ccs.Entities;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class AnalysisOneModel
    {
        public Dictionary<string, int> AcceptedLanguages { get; } = new Dictionary<string, int>();

        public Dictionary<string, int> AttemptedLanguages { get; } = new Dictionary<string, int>();

        public Dictionary<int, int> AcceptedProblems { get; } = new Dictionary<int, int>();

        public Dictionary<int, int> AttemptedProblems { get; } = new Dictionary<int, int>();

        public Dictionary<int, int> TeamLastSubmission { get; } = new Dictionary<int, int>();

        public Dictionary<(int TeamId, int ProblemId), (int ac, int at)> TeamStatistics { get; } = new Dictionary<(int, int), (int, int)>();

        public int TotalMinutes { get; }

        public int TotalSubmissions { get; set; }

        public int[,] VerdictStatistics { get; }

        public IReadOnlyDictionary<int, Team> Teams { get; set; }

        public ProblemCollection Problems { get; set; }

        public AnalysisOneModel(int time)
        {
            TotalMinutes = time;
            VerdictStatistics = new int[12, time + 1];
        }
    }
}
