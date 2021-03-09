using Ccs.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Plag.Backend.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ccs.Connector.PlagiarismDetect.Models
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
