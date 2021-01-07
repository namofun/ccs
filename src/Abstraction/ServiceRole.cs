using Microsoft.Extensions.DependencyInjection;

namespace Ccs
{
    /// <summary>
    /// The role definition for contest related operations.
    /// </summary>
    public interface IServiceRole
    {
        /// <summary>
        /// Configure the contest storage implementations.
        /// </summary>
        /// <param name="services">The service collection.</param>
        void Configure(IServiceCollection services);
    }
}
