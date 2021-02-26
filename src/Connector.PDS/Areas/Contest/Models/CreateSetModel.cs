using System.ComponentModel.DataAnnotations;

namespace Ccs.Connector.PlagiarismDetect.Models
{
    public class CreateSetModel
    {
        [Required]
        public string Name { get; set; }
    }
}
