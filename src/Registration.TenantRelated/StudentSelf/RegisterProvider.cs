using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Xylab.Contesting.Registration
{
    public class StudentSelfRegisterProvider : ContestantRegisterProviderBase<EmptyModel>
    {
        public override int Order => -800;

        public override string Name => "student";

        public override string Icon => string.Empty;

        public override string FancyName => "student-self";

        protected override Task<EmptyModel> CreateInputModelAsync(RegisterProviderContext context)
            => EmptyModel.CompletedTask;

        protected override bool IsAvailable(RegisterProviderContext context)
            => base.IsAvailable(context) && context.TryGetStudent(out _, out _, out _, out _);

        protected override async Task<StatusMessageModel> ExecuteAsync(RegisterProviderContext context, EmptyModel model)
        {
            if (!context.TryGetStudent(out int affId, out var studId, out _, out var studName))
            {
                throw new NotImplementedException("Unknown error occurred.");
            }

            // This time, current user haven't got any team registered.
            var teamName = studId + studName;
            var user = new FakeRegisterUser(int.Parse(context.User.GetUserId()!));

            var t = await context.CreateTeamAsync(
                users: new[] { user },
                team: new Entities.Team
                {
                    ContestId = context.Contest.Id,
                    TeamName = teamName,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 1,
                    AffiliationId = affId,
                    CategoryId = context.Contest.Settings.RegisterCategory![FancyName],
                });

            return StatusMessageModel.Succeed(t);
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<EmptyModel> output)
        {
            if (!context.TryGetStudent(out _, out var studId, out var affName, out var studName))
                throw new InvalidOperationException("Unknown error occurred.");

            output.AppendHtml("<p>You are going to be registered as <b>")
                .Append(studId + studName)
                .AppendHtml("</b> from ")
                .Append(affName!)
                .AppendHtml(".</p>")
                .AppendAgreement(
@"The registration confirms that you:
* have read the contest rules
* will not violate the rules
* will not communicate with other students, use another student's code for solutions/generators, share ideas of solutions
* will not attempt to deliberately destabilize the testing process and try to hack the contest system in any form.");

            return Task.CompletedTask;
        }

        protected override Task ValidateAsync(RegisterProviderContext context, EmptyModel model, ModelStateDictionary modelState)
            => Task.CompletedTask;
    }
}
