using Ccs.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ccs.Contexts
{
    public class ImmediateContestContextFactory : IContestContextFactory
    {
        public async Task<IContestContext> CreateAsync(int cid, IServiceProvider serviceProvider)
        {
            var cst = await serviceProvider.GetRequiredService<IContestStore>().FindAsync(cid);
            return new Immediate.ImmediateContestContext(cst, serviceProvider);
        }
    }
}
