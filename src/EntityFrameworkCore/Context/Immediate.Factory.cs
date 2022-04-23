using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Xylab.Contesting.Services
{
    public class ImmediateContestContextFactory : IContestContextFactory
    {
        public async Task<IContestContext?> CreateAsync(int cid, IServiceProvider serviceProvider, bool requireProblems = true)
        {
            var cst = await serviceProvider.GetRequiredService<IContestRepository>().FindAsync(cid);
            if (cst == null) return null;
            return new ImmediateContestContext(cst, serviceProvider);
        }
    }
}
