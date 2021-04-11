using Ccs.Entities;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class ChooseProblemModel
    {
        public Dictionary<int, ContestProblem> Problems { get; set; }
    }
}
