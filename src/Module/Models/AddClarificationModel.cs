using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.ContestModule.Models
{
    public class AddClarificationModel
    {
        public int? ReplyTo { get; set; }

        [DisplayName("Send to")]
        public int TeamTo { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Body { get; set; }

        [DisplayName("Subject")]
        public string Type { get; set; }
    }
}
