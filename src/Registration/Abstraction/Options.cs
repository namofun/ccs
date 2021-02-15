using System.Collections.Generic;

namespace Ccs.Registration
{
    /// <summary>
    /// The option class for registration providers.
    /// </summary>
    public class ContestRegistrationOptions
    {
        private readonly List<(string, IRegisterProvider)> _providers
            = new List<(string, IRegisterProvider)>();

        /// <summary>
        /// The providers.
        /// </summary>
        public IReadOnlyList<(string, IRegisterProvider)> Providers => _providers;

        /// <summary>
        /// Adds a register provider to the list.
        /// </summary>
        /// <param name="provider">The provider instance.</param>
        public void Add(IRegisterProvider provider)
            => _providers.Add(
                (FluentTagExtensions.NotNull(provider, nameof(provider)).FancyName,
                 provider));

        /// <summary>
        /// Completes the options.
        /// </summary>
        public void Complete()
            => _providers.Sort((a, b) => a.Item2.Order.CompareTo(b.Item2.Order));
    }
}
