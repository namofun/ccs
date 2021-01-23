using System.ComponentModel;

namespace SatelliteSite.ContestModule.Models
{
    public class CodeSubmitModel
    {
        [DisplayName("Your Code")]
        public string Code { get; set; }

        [DisplayName("Language")]
        public string Language { get; set; }

        public string ProblemId { get; set; }
    }
}
