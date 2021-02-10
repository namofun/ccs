using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewLanguageModel
    {
        public Language Language { get; set; }

        public List<Solution> Solutions { get; set; }
    }
}
