using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Registration.BatchByTeamName
{
    public class BatchByTeamNameRegisterProvider : IRegisterProvider<BatchByTeamNameInputModel, BatchByTeamNameOutputModel>
    {
        public bool JuryOrContestant => true;

        public async Task<BatchByTeamNameInputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            return new BatchByTeamNameInputModel
            {
                Affiliations = await context.Context.FetchAffiliationsAsync(false),
                Categories = await context.Context.FetchCategoriesAsync(false),
            };
        }

        public async Task ValidateAsync(RegisterProviderContext context, BatchByTeamNameInputModel model, ModelStateDictionary modelState)
        {
            model.Affiliations ??= await context.Context.FetchAffiliationsAsync(false);
            model.Categories ??= await context.Context.FetchCategoriesAsync(false);

            if (!model.Categories.ContainsKey(model.CategoryId))
            {
                modelState.AddModelError("bbtnrp::nocat", "Category not found.");
            }

            if (!model.Affiliations.ContainsKey(model.AffiliationId))
            {
                modelState.AddModelError("bbtnrp::noaff", "Affiliation not found.");
            }

            if (string.IsNullOrWhiteSpace(model.TeamNames))
            {
                modelState.AddModelError("bbtnrp::notn", "No team name specified.");
            }
        }

        public Task<BatchByTeamNameOutputModel> ExecuteAsync(RegisterProviderContext context, BatchByTeamNameInputModel model)
        {

            throw new NotImplementedException();
        }

        public Task RenderInputAsync(
            RegisterProviderContext context,
            RegisterProviderOutput<BatchByTeamNameInputModel> output)
        {
            output.WithTitle("Batch team register")
                .AppendValidationSummary()
                .AppendSelect(
                    @for: __model => __model.AffiliationId,
                    items: output.Model.Affiliations.Values.Select(a => new SelectListItem(a.Name, $"{a.Id}")))
                .AppendSelect(
                    @for: __model => __model.CategoryId,
                    items: output.Model.Categories.Values.Select(c => new SelectListItem(c.Name, $"{c.Id}")))
                .AppendTextArea(
                    @for: __model => __model.TeamNames,
                    comment: "队伍名称，每个一行，区分大小写和空格，提交后会绑定新用户并重置密码。");

            return Task.CompletedTask;
        }

        public Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<BatchByTeamNameOutputModel> output)
        {
            output.WithTitle("Batch import result")
                .AppendDataTable(
                    elements: output.Model,
                    tableClass: "text-center table-hover",
                    theadClass: "thead-light");

            return Task.CompletedTask;
        }
    }
}
