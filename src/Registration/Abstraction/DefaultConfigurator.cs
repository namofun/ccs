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
            options.Add(new BatchByTeamNameRegisterProvider());
            options.Add(new IndividualParticipantRegisterProvider("individual-participant", 1));
            options.Add(new IndividualParticipantRegisterProvider("individual-part-verify", 0));
        }

        /// <inheritdoc />
        public void PostConfigure(string name, ContestRegistrationOptions options)
        {
            options.Complete();
        }
    }
}
