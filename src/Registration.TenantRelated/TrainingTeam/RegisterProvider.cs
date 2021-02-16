using Ccs.Registration.TrainingTeam;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Services;

namespace Ccs.Registration
{
    public class TrainingTeamRegisterProvider : ContestantRegisterProviderBase<InputModel>
    {
        public override int Order => -1000;

        public override string Name => "team member";

        public override string Icon => string.Empty;

        public override string FancyName { get; }

        private int DefaultStatus { get; }

        public TrainingTeamRegisterProvider(string fancyName, int defaultStatus)
        {
            FancyName = fancyName;
            DefaultStatus = defaultStatus;
        }

        protected override Task<InputModel> CreateInputModelAsync(RegisterProviderContext context)
            => Task.FromResult(new InputModel());

        protected override async Task<StatusMessageModel> ExecuteAsync(RegisterProviderContext context, InputModel model)
        {
            // This time, current users haven't got any team registered.
            var t = await context.CreateTeamAsync(
                users: model.UserIds.Select(i => new FakeRegisterUser(i)),
                team: new Entities.Team
                {
                    ContestId = context.Contest.Id,
                    TeamName = model.TeamName,
                    RegisterTime = DateTimeOffset.Now,
                    Status = DefaultStatus,
                    AffiliationId = model.AffiliationId,
                    CategoryId = context.Contest.Settings.RegisterCategory![FancyName],
                });

            return StatusMessageModel.Succeed(t);
        }

        protected override async Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<InputModel> output)
        {
            var training = context.GetRequiredService<IGroupStore>();
            var items = await training.ListByUserAsync(int.Parse(context.User.GetUserId()!), true);

            var teamsJson = items.Select(g => new
            {
                team = new { name = g.Key.TeamName, id = g.Key.Id },
                users = g.Select(v => new { name = v.UserName, id = v.UserId }).ToList()
            })
            .ToJson();

            var script = new TagBuilder("script");
            script.InnerHtml.AppendHtml(@"
$(function () {
    var teamInfo = ").AppendHtml(teamsJson).AppendHtml(@";
    for (var i = 0; i < teamInfo.length; i++) {
        $('#team-ids').append('<option value=""' + teamInfo[i].team.id + '"">' + teamInfo[i].team.name + '</option>');
    }

    $('#team-ids').on('change', function () {
        $('#team-members').html('');
        if ($('#team-ids').val() === '0') return;
        for (var i = 0; i < teamInfo.length; i++) {
            if (teamInfo[i].team.id != $('#team-ids').val()) continue;
            for (var j = 0; j < teamInfo[i].users.length; j++) {
                $('#team-members').append(
                    '<div class=""custom-control custom-checkbox ml-4"">' +
                        '<input type=""checkbox"" class=""custom-control-input sel-user"" name=""").AppendHtml(output.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix + ".").AppendHtml(@"UserIds[]"" id=""sel-user' + teamInfo[i].users[j].id + '"" value=""' + teamInfo[i].users[j].id + '"">' +
                        '<label class=""custom-control-label"" for=""sel-user' + teamInfo[i].users[j].id + '"">' + teamInfo[i].users[j].name + '</label>' +
                    '</div>');
            }
        }
        $('.sel-user').attr('checked', true);
        $('#sel-user").AppendHtml(context.User.GetUserId()).AppendHtml(@"').attr('disabled', true);
    });
});");

            var @for = output.ModelExpressionProvider.CreateModelExpression(
                output.ViewData,
                __model => __model.TeamId);

            var select = output.Generator.GenerateSelect(
                output.ViewContext,
                @for.ModelExplorer,
                optionLabel: null,
                expression: @for.Name,
                selectList: Enumerable.Empty<SelectListItem>(),
                currentValues: Array.Empty<string>(),
                allowMultiple: false,
                htmlAttributes: null)
                ?? throw new InvalidOperationException("Generation for select got wrong.");

            select.InnerHtml.AppendHtml("<option>select a team...</option>");
            select.Attributes["id"] = "team-ids";
            select.AddCssClass("form-control mb-2");

            output.AppendAgreement(
@"The registration confirms that you:
* have read the contest rules
* will not violate the rules
* will not communicate with other participants, use another person's code for solutions/generators, share ideas of solutions and hacks
* will not attempt to deliberately destabilize the testing process and try to hack the contest system in any form.")
                .AppendHtml("<div class=\"row\" id=\"team-selection\"><div class=\"col-10 col-lg-6\">")
                .AppendHtml(select)
                .AppendHtml("<div class=\"d-flex mb-2\" style=\"margin-left:-1.5rem\" id=\"team-members\"></div></div></div>")
                .AppendHtml(script);
        }

        protected override async Task ValidateAsync(RegisterProviderContext context, InputModel model, ModelStateDictionary modelState)
        {
            var training = context.GetRequiredService<IGroupStore>();
            var team = await training.FindByIdAsync(model.TeamId);
            if (team == null)
            {
                modelState.AddModelError("NoTeam", "Team not found.");
                return;
            }

            int uid = int.Parse(context.User.GetUserId()!);
            var users = await training.ListMembersAsync(team, true);
            var uids = (model.UserIds ?? Enumerable.Empty<int>()).Append(uid).Distinct().ToList();
            if (uids.Except(users.Select(g => g.UserId)).Any())
            {
                modelState.AddModelError("MoreMember", "Team member not found.");
            }

            foreach (var item in uids)
            {
                if (await context.Contest.Context.FindMemberByUserAsync(item) != null)
                {
                    var u = users.Single(u => u.UserId == item).UserName;
                    modelState.AddModelError("RegMember", $"One of members {u} has registered this contest.");
                }
            }

            model.TeamName = team.TeamName;
            model.AffiliationId = team.AffiliationId;
            model.UserIds = uids;
        }
    }
}
