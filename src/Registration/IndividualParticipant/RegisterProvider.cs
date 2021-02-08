﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    public class IndividualParticipantRegisterProvider : ContestantRegisterProviderBase<EmptyModel>
    {
        public override int Order => -1000;

        public override string Name => "individual participant";

        public override string Icon => string.Empty;

        protected override Task<EmptyModel> CreateInputModelAsync(RegisterProviderContext context)
            => EmptyModel.CompletedTask;

        protected override async Task<StatusMessageModel> ExecuteAsync(RegisterProviderContext context, EmptyModel model)
        {
            // This time, current user haven't got any team registered.
            var teamName = context.User.GetNickName()!;
            var user = new FakeRegisterUser(int.Parse(context.User.GetUserId()!));

            await context.CreateTeamAsync(
                users: new[] { user },
                team: new Entities.Team
                {
                    ContestId = context.Contest.Id,
                    TeamName = teamName,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 0,
                    AffiliationId = -1,
                    CategoryId = 0,
                });

            return new StatusMessageModel(true, "Register succeeded.");
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<EmptyModel> output)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateAsync(RegisterProviderContext context, EmptyModel model, ModelStateDictionary modelState)
            => Task.CompletedTask;
    }
}