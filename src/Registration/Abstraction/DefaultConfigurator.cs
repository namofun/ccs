using Microsoft.Extensions.Options;

namespace Ccs.Registration
{
    internal class DefaultConfigurator :
        IConfigureOptions<ContestRegistrationOptions>,
        IPostConfigureOptions<ContestRegistrationOptions>
    {
        public void Configure(ContestRegistrationOptions options)
        {
            options.Add("batch-by-name", new BatchByTeamNameRegisterProvider());
        }

        public void PostConfigure(string name, ContestRegistrationOptions options)
        {
            options.Complete();
        }
    }
}
