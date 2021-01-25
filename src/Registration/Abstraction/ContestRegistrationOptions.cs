using System.Collections.Generic;

namespace Ccs.Registration
{
    /// <summary>
    /// The option class for registration providers.
    /// </summary>
    public class ContestRegistrationOptions
    {
        /// <summary>
        /// The providers.
        /// </summary>
        public IList<IRegisterProvider> Providers { get; set; }

        /// <summary>
        /// Initialize the options.
        /// </summary>
        public ContestRegistrationOptions()
            => Providers = new List<IRegisterProvider>();
    }
}
