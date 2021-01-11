using Ccs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual async Task<List<BalloonModel>> FetchBalloonsAsync()
        {
            var problems = await FetchProblemsAsync();
            return await Ccs.BalloonStore.ListAsync(Contest, problems);
        }

        public virtual Task SetBalloonDoneAsync(int id)
        {
            return Ccs.BalloonStore.SetDoneAsync(Contest, id);
        }
    }
}
