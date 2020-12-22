using Ccs.Models;
using System.Collections;

namespace SatelliteSite.ContestModule.Models
{
    public class AnalysisTwoModel
    {
        public int TotalMinutes { get; }

        public int TotalSubmissions { get; set; }

        public int TotalAccepted { get; set; }

        public int TeamAttempted { get; set; }

        public int TeamAccepted { get; set; }

        public int[,] VerdictStatistics { get; }

        public IEnumerable List { get; set; }

        public ProblemModel Problem { get; }

        public AnalysisTwoModel(int time, ProblemModel cp)
        {
            TotalMinutes = time;
            VerdictStatistics = new int[12, time + 1];
            Problem = cp;
        }
    }
}
