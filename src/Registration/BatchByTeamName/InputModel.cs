#nullable disable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel;
using Tenant.Entities;

namespace Ccs.Registration.BatchByTeamName
{
    public class BatchByTeamNameInputModel
    {
        [BindNever]
        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; set; }

        [BindNever]
        public IReadOnlyDictionary<int, Category> Categories { get; set; }

        [DisplayName("Affiliation")]
        public int AffiliationId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [DisplayName("Team Names")]
        public string TeamNames { get; set; }
    }
}
