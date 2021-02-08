using Ccs;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Models
{
    public class AddRejudgingModel
    {
        [Required]
        public string Reason { get; set; }

        public string[] Judgehosts { get; set; }

        public Verdict[] Verdicts { get; set; }

        public int[] Problems { get; set; }

        public string[] Languages { get; set; }

        public int[] Teams { get; set; }

        public int? Submission { get; set; }

        [TimeSpan]
        public string TimeBefore { get; set; }

        [TimeSpan]
        public string TimeAfter { get; set; }

        [BindNever]
        [ValidateNever]
        public IReadOnlyList<Language> AllowedLanguages { get; private set; }

        [BindNever]
        [ValidateNever]
        public IReadOnlyList<Judgehost> AllowedJudgehosts { get; private set; }

        [BindNever]
        [ValidateNever]
        public IReadOnlyDictionary<int, string> AllowedTeamNames { get; private set; }

        [BindNever]
        [ValidateNever]
        public ProblemCollection AllowedProblems { get; private set; }

        [Obsolete("Problem listing need refactor")]
        public async Task<AddRejudgingModel> LoadAsync(IRejudgingContext context)
        {
            AllowedTeamNames ??= await context.ListTeamNamesAsync();
            AllowedJudgehosts ??= await context.GetJudgehostsAsync();
            AllowedLanguages ??= await context.ListLanguagesAsync();
            AllowedProblems ??= await context.ListProblemsAsync();
            return this;
        }
    }
}
