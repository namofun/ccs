using Ccs.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Polygon.Entities;
using System.Collections.Generic;
using System.ComponentModel;

namespace SatelliteSite.ContestModule.Models
{
    public class TeamCodeSubmitModel
    {
        [DisplayName("Problem")]
        public string Problem { get; set; }

        [DisplayName("Language")]
        public string Language { get; set; }

        [DisplayName("Source Code")]
        public string Code { get; set; }

        [BindNever]
        [ValidateNever]
        public ProblemCollection Problems { get; set; }

        [BindNever]
        [ValidateNever]
        public IReadOnlyList<Language> Languages { get; set; }
    }
}
