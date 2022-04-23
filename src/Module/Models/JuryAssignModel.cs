using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xylab.Contesting.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryAssignModel
    {
        [DisplayName("User Name")]
        [Required]
        public string UserName { get; set; }

        public JuryLevel Level { get; set; }
    }
}
