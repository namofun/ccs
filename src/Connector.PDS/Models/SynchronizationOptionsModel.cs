using Ccs.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Ccs.Connector.PlagiarismDetect.Models
{
    public class SynchronizationOptionsModel
    {
        [BindNever]
        [ValidateNever]
        public ProblemCollection Problems { get; set; }

        public int[] ChosenProblems { get; set; }
    }
}
