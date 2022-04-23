using System.ComponentModel.DataAnnotations;

namespace Xylab.Contesting.Connector.PlagiarismDetect.Models
{
    public class CreateSetModel
    {
        [Required]
        public string Name { get; set; }
    }
}
