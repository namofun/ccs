using System.Collections.Generic;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewLanguageModel
    {
        public Language Language { get; set; }

        public List<Solution> Solutions { get; set; }
    }
}
