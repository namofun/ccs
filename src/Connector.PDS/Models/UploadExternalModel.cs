using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xylab.Contesting.Models;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.Contesting.Connector.PlagiarismDetect.Models
{
    public class UploadExternalModel
    {
        [BindNever]
        [ValidateNever]
        public List<LanguageInfo> AvailableLanguages { get; set; }

        [BindNever]
        [ValidateNever]
        public ProblemCollection AvailableProblems { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Content { get; set; }

        public int Problem { get; set; }
    }
}
