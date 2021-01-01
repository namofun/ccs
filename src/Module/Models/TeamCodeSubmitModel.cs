using System.ComponentModel;

namespace SatelliteSite.ContestModule.Models
{
    public class TeamCodeSubmitModel
    {
        [DisplayName("Problem")]
        public string Problem { get; set; }

        [DisplayName("Language")]
        public string Language { get; set; }

        [DisplayName("Source Code")]
        public string Code { get; set; }
    }
}
