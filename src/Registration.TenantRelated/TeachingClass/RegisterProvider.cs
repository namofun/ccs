using Ccs.Registration.TeachingClass;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    public class TeachingClassRegisterProvider : RegisterProviderBase<InputModel, OutputModel>
    {
        public override int Order => -500;

        public override string Name => "Teaching class";

        public override string Icon => "fas fa-graduation-cap";

        public override bool JuryOrContestant => true;

        protected override Task<InputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateAsync(RegisterProviderContext context, InputModel model, ModelStateDictionary modelState)
        {
            throw new NotImplementedException();
        }

        protected override Task<OutputModel> ExecuteAsync(RegisterProviderContext context, InputModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<InputModel> output)
        {
            throw new NotImplementedException();
        }

        protected override Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<OutputModel> output)
        {
            throw new NotImplementedException();
        }
    }
}
