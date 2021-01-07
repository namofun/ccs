using Ccs.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ccs.Contexts
{
    /// <summary>
    /// The interface for creating <see cref="IContestContext"/>.
    /// </summary>
    /// <remarks>
    /// This service should be registered as <see cref="ServiceLifetime.Singleton"/>.
    /// That means the implementation can't depend on any <see cref="ServiceLifetime.Scoped"/> services.
    /// </remarks>
    public interface IContestContextFactory
    {
        /// <summary>
        /// Creates an <see cref="IContestContext"/> for reading contest informations.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="serviceProvider">The scoped service provider. This parameter usually comes from <c>HttpContext.RequestServices</c>.</param>
        /// <returns>The task for creating contest context.</returns>
        Task<IContestContext?> CreateAsync(int cid, IServiceProvider serviceProvider);
    }

    /// <summary>
    /// The interface for creating <see cref="IContestContext"/>.
    /// </summary>
    /// <remarks>
    /// This service should be registered as <see cref="ServiceLifetime.Scoped"/>.
    /// This is for scoped services to access the singleton <see cref="IContestContextFactory"/>.
    /// </remarks>
    public class ScopedContestContextFactory
    {
        /// <summary>
        /// The scoped service provider
        /// </summary>
        internal IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Instantiate the <see cref="ScopedContestContextFactory"/>.
        /// </summary>
        /// <param name="serviceProvider">The scoped service provider.</param>
        public ScopedContestContextFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates an <see cref="IContestContext"/> for reading contest informations.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for creating contest context.</returns>
        public Task<IContestContext?> CreateAsync(int cid)
        {
            return ServiceProvider
                .GetRequiredService<IContestContextFactory>()
                .CreateAsync(cid, ServiceProvider);
        }

        /// <summary>
        /// Get the scoreboard store.
        /// </summary>
        /// <returns>The scoreboard store.</returns>
        public IScoreboard CreateScoreboard()
        {
            return ServiceProvider
                .GetRequiredService<IScoreboard>();
        }
    }
}
