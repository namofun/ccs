using Ccs.Services;
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

        public IReadOnlyList<Language> AllowedLanguages { get; private set; }

        public IReadOnlyList<Judgehost> AllowedJudgehosts { get; private set; }

        public IReadOnlyDictionary<int, string> AllowedTeamNames { get; private set; }

        public async Task<AddRejudgingModel> LoadAsync(IRejudgingContext context)
        {
            AllowedTeamNames ??= await context.FetchTeamNamesAsync();
            AllowedJudgehosts ??= await context.FetchJudgehostsAsync();
            AllowedLanguages ??= await context.FetchLanguagesAsync();
            return this;
        }
    }
}
