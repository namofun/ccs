using System.ComponentModel;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryAddTeamModel : JuryEditTeamModel
    {
        [DisplayName("User name")]
        public string UserName { get; set; }
    }
}
