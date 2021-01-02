using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class GymRegisterModel
    {
        public bool AsIndividual { get; set; }

        public int TeamId { get; set; }

        public List<int> UserIds { get; set; }
    }
}
