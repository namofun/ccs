using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Xylab.Contesting.Registration
{
    public class IndividualParticipantRegisterProvider : ContestantRegisterProviderBase<EmptyModel>
    {
        public override int Order => -1000;

        public override string Name => "individual participant";

        public override string Icon => string.Empty;

        public override string FancyName { get; }

        private int DefaultStatus { get; }

        public IndividualParticipantRegisterProvider(string fancyName, int defaultStatus)
        {
            FancyName = fancyName;
            DefaultStatus = defaultStatus;
        }

        protected override Task<EmptyModel> CreateInputModelAsync(RegisterProviderContext context)
            => EmptyModel.CompletedTask;

        protected override async Task<StatusMessageModel> ExecuteAsync(RegisterProviderContext context, EmptyModel model)
        {
            // This time, current user haven't got any team registered.
            var teamName = context.User.GetNickName()!;
            var user = new FakeRegisterUser(int.Parse(context.User.GetUserId()!));

            var t = await context.CreateTeamAsync(
                users: new[] { user },
                team: new Entities.Team
                {
                    ContestId = context.Contest.Id,
                    TeamName = teamName,
                    RegisterTime = DateTimeOffset.Now,
                    Status = DefaultStatus,
                    AffiliationId = -1, // (none), should've existed
                    CategoryId = context.Contest.Settings.RegisterCategory![FancyName],
                });

            return StatusMessageModel.Succeed(t);
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<EmptyModel> output)
        {
            output.AppendAgreement(
@"The registration confirms that you:
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
