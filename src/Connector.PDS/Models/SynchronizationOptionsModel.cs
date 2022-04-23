using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Connector.PlagiarismDetect.Models
{
    public class SynchronizationOptionsModel
    {
        [BindNever]
        [ValidateNever]
        public ProblemCollection Problems { get; set; }

        public int[] ChosenProblems { get; set; }
    }
}
