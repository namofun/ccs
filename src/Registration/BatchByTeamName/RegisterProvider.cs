using Ccs.Models;
using System;
using System.Threading.Tasks;

namespace Ccs.Registration.BatchByTeamName
{
    public class BatchByTeamNameRegisterProvider : IRegisterProvider<BatchByTeamNameInputModel, BatchByTeamNameOutputModel>
    {
        public bool JuryOrContestant => true;

        public Task<BatchByTeamNameInputModel> CreateInputModelAsync(RegisterProviderContext context)
        {
            throw new NotImplementedException();
        }

        public Task<BatchByTeamNameOutputModel> ExecuteAsync(RegisterProviderContext context, BatchByTeamNameInputModel model)
        {
            throw new NotImplementedException();
        }

        public Task RenderInputAsync(RegisterProviderContext context, BatchByTeamNameInputModel model, RegisterProviderOutput<BatchByTeamNameInputModel> output)
        {
            throw new NotImplementedException();
        }

        public Task RenderOutputAsync(RegisterProviderContext context, BatchByTeamNameOutputModel model, RegisterProviderOutput<BatchByTeamNameOutputModel> output)
        {
            throw new NotImplementedException();
        }

        public Task<CheckResult> ValidateAsync(RegisterProviderContext context, BatchByTeamNameInputModel model)
        {
            throw new NotImplementedException();
        }
    }
}
