#nullable disable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace Ccs.Registration.TrainingTeam
{
    public class InputModel
    {
        public int TeamId { get; set; }

        public List<int> UserIds { get; set; }

        [BindNever]
        [ValidateNever]
        public int AffiliationId { get; set; }

        [BindNever]
        [ValidateNever]
        public string TeamName { get; set; }
    }
}
