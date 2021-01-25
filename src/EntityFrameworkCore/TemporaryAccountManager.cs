using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class TemporaryAccountManager
    {
        public IUserManager UserManager { get; }

        public TemporaryAccountManager(IUserManager userManager)
        {
            UserManager = userManager;
        }

        /*
        public Task<int> BatchClearAsync(Contest contest)
        {
            throw new NotImplementedException();
        }

        public async Task<int> BatchLockOutAsync(int cid)
        {
            var lockOuts = Context.Set<Member>()
                .Where(m => m.ContestId == cid && m.Temporary);

            await UserManager.BatchLockOutAsync(lockOuts.Select(m => m.UserId));

            await Context.Set<TUser>()
                .Where(u => lockOuts2.Contains(u.Id))
                .BatchUpdateAsync(u => new TUser { LockoutEnd = DateTimeOffset.MaxValue });

            return await lockOuts.BatchDeleteAsync();
        }
        */
    }
}
