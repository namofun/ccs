using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class GymFilteringModel
    {
        public string Language { get; set; }

        public string Problem { get; set; }

        public Verdict? Verdict { get; set; }

        public IDictionary<string, string> ToRouteData()
        {
            var dict = new Dictionary<string, string>();
            if (Language != null) dict.Add("lang", Language);
            if (Problem != null) dict.Add("prob", Problem);
            if (Verdict.HasValue) dict.Add("verd", ((int)Verdict.Value).ToString());
            if (dict.Count > 0) dict.Add("filter", "1");
            return dict;
        }
    }
}
