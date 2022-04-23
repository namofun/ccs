using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class GymHomeViewModel
    {
        public IReadOnlyList<Clarification> Clarifications { get; set; }

        public string Markdown { get; set; }

        public IReadOnlyDictionary<int, (int Accepted, int Total)> MeStatistics { get; set; }

        public IReadOnlyDictionary<int, (int Accepted, int Total, int AcceptedTeams, int TotalTeams)> AllStatistics { get; set; }

        public ProblemCollection Problems { get; set; }
    }
}
