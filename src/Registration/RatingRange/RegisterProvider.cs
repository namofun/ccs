using Ccs.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    public class RatingRangeRegisterProvider : ContestantRegisterProviderBase<EmptyModel>
    {
        public override int Order => -2000;

        public override string Name => "individual participant";

        public override string Icon => string.Empty;

        public override string FancyName { get; }

        public int? RatingAtLeast { get; }

        public int? RatingAtMost { get; }

        public string RatedFor { get; }

        public bool OutOfCompetition { get; }

        public static IReadOnlyList<RatingRangeRegisterProvider> Presets { get; }
            = new RatingRangeRegisterProvider[]
            {
                new RatingRangeRegisterProvider("div-1and2", "all participants", false, null, null),
                new RatingRangeRegisterProvider("div1-only", "Div.1 participants", false, 1900, null),
                new RatingRangeRegisterProvider("div2-only", "Div.2 participants", false, null, 1900),
                new RatingRangeRegisterProvider("div2-open", "Div.2 participants", true, null, 2100),
                new RatingRangeRegisterProvider("div3-open", "Div.3 participants", true, null, 1600),
            };

        public RatingRangeRegisterProvider(string fancyName, string ratedFor, bool outOfCompetition, int? lower, int? upper)
        {
            FancyName = fancyName;
            RatedFor = ratedFor;
            OutOfCompetition = outOfCompetition;
            RatingAtLeast = lower;
            RatingAtMost = upper;
        }

        protected override Task<EmptyModel> CreateInputModelAsync(RegisterProviderContext context)
            => EmptyModel.CompletedTask;

        protected override bool IsAvailable(RegisterProviderContext context)
            => CcsDefaults.SupportsRating
                && base.IsAvailable(context)
                && (OutOfCompetition
                    || (int.TryParse(context.User.FindFirst(RatingClaimsProvider.RatingClaimsName)?.Value ?? "1500", out int rating)
                        && !(RatingAtLeast > rating || RatingAtMost <= rating)));

        protected override async Task<StatusMessageModel> ExecuteAsync(RegisterProviderContext context, EmptyModel model)
        {
            var ratingClaim = context.User.FindFirst(RatingClaimsProvider.RatingClaimsName);
            var rating = int.Parse(ratingClaim?.Value ?? "1500");
            var category = RatingAtLeast > rating || RatingAtMost <= rating ? -4 : -3; // -4 => Observers, -3 => Participants

            var team = new Entities.Team
            {
                AffiliationId = -1,
                CategoryId = category,
                ContestId = context.Contest.Id,
                Status = 1,
                TeamName = context.User.GetUserName()!,
            };

            var users = new[]
            {
                new FakeRegisterUserWithRating(
                    int.Parse(context.User.GetUserId()!),
                    ratingClaim == null ? default(int?) : rating)
            };

            await context.CreateTeamAsync(team, users);
            return StatusMessageModel.Succeed(team);
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<EmptyModel> output)
        {
            output.AppendAgreement(
@$"This contest is rated for {RatedFor}.{(OutOfCompetition ? " Others can take part in this contest out of competition." : "")}

The registration confirms that you:
* have read the contest rules
* will not violate the rules
* will not communicate with other participants, use another person's code for solutions/generators, share ideas of solutions and hacks
* will not attempt to deliberately destabilize the testing process and try to hack the contest system in any form
* will not use multiple accounts and will take part in the contest using your personal and the single account.");

            return Task.CompletedTask;
        }

        protected override Task ValidateAsync(RegisterProviderContext context, EmptyModel model, ModelStateDictionary modelState)
            => Task.CompletedTask;
    }
}
