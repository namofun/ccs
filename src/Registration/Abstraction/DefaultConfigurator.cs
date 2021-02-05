using Microsoft.Extensions.Options;

namespace Ccs.Registration
{
    /// <summary>
    /// Configure the default registration.
    /// </summary>
    internal class DefaultConfigurator :
        IConfigureOptions<ContestRegistrationOptions>,
        IPostConfigureOptions<ContestRegistrationOptions>
    {
        /// <inheritdoc />
        public void Configure(ContestRegistrationOptions options)
        {
            options.Add("batch-by-name", new BatchByTeamNameRegisterProvider());
            options.Add("individual-participant", new IndividualParticipantRegisterProvider());
        }

        /// <inheritdoc />
        public void PostConfigure(string name, ContestRegistrationOptions options)
        {
            options.Complete();
        }
    }
}
