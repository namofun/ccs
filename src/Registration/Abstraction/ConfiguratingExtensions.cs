using Ccs.Registration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extensions for contest registration.
    /// </summary>
    public static class RegistrationConfiguratingExtensions
    {
        /// <summary>
        /// Adds the <see cref="ContestRegistrationOptions"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The same <see cref="IServiceCollection"/> to chain operations.</returns>
        public static IServiceCollection AddContestRegistration(this IServiceCollection services)
        {
            return services.ConfigureOptions<DefaultConfigurator>();
        }

        /// <summary>
        /// Adds the <see cref="IRegisterProvider"/> to the <see cref="ContestRegistrationOptions"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The provider name.</param>
        /// <param name="provider">The <see cref="IRegisterProvider"/>.</param>
        /// <returns>The same <see cref="IServiceCollection"/> to chain operations.</returns>
        public static IServiceCollection AddContestRegistrationProvider(this IServiceCollection services, string name, IRegisterProvider provider)
        {
            return services.Configure<ContestRegistrationOptions>(options => options.Add(name, provider));
        }

        /// <summary>
        /// Adds the <typeparamref name="TProvider"/> to the <see cref="ContestRegistrationOptions"/>.
        /// </summary>
        /// <typeparam name="TProvider">The register provider type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The provider name.</param>
        /// <returns>The same <see cref="IServiceCollection"/> to chain operations.</returns>
        public static IServiceCollection AddContestRegistrationProvider<TProvider>(this IServiceCollection services, string name) where TProvider : IRegisterProvider, new()
        {
            return AddContestRegistrationProvider(services, name, new TProvider());
        }
    }
}
