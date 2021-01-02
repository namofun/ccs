using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryAssignModel
    {
        [DisplayName("User Name")]
        [Required]
        public string UserName { get; set; }
    }
}
