using System;
using System.Collections.Generic;
using System.Text;

namespace Ccs.Registration
{
    /// <summary>
    /// The execution context for register provider.
    /// </summary>
    public class RegisterProviderContext
    {
        /// <summary>
        /// Provides contest context.
        /// </summary>
        public IContestContext Context { get; }

        public RegisterProviderContext(IContestContext context)
        {
            Context = context;
        }
    }
}
