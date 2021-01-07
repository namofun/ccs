using Ccs.Entities;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    public class TemporaryAccountManager
    {
        public IUserManager UserManager { get; }

        public ITeamStore TeamStore { get; }

        public TemporaryAccountManager(IUserManager userManager, ITeamStore teamStore)
        {
            UserManager = userManager;
            TeamStore = teamStore;
        }

        public static Func<string> CreatePasswordGenerator()
        {
            const string passwordSource = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var rng = new Random(unchecked((int)DateTimeOffset.Now.Ticks));
            return () =>
            {
                Span<char> pwd = stackalloc char[8];
                for (int i = 0; i < 8; i++) pwd[i] = passwordSource[rng.Next(passwordSource.Length)];
                return pwd.ToString();
            };
        }

        public static string UserNameForTeamId(int teamId) => $"team{teamId:D3}";

        public async Task EnsureTeamWithPassword(int cid, int teamId, string password)
        {
            string username = UserNameForTeamId(teamId);

            var user = await UserManager.FindByNameAsync(username);

            if (user != null)
            {
                if (await UserManager.HasPasswordAsync(user))
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                    await UserManager.ResetPasswordAsync(user, token, password);
                }
                else
                {
                    await UserManager.AddPasswordAsync(user, password);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    await UserManager.SetLockoutEndDateAsync(user, null);
                }
            }
            else
            {
                user = UserManager.CreateEmpty(username);
                user.Email = $"{username}@contest.acm.xylab.fun";
                await UserManager.CreateAsync(user, password);
            }

            /*
            await Context.Set<Member>().MergeAsync(
                sourceTable: new[] { new { ContestId = cid, TeamId = teamId, UserId = user.Id } },
                targetKey: t => new { t.ContestId, t.TeamId, t.UserId },
                sourceKey: t => new { t.ContestId, t.TeamId, t.UserId },
                insertExpression: t => new Member { ContestId = t.ContestId, TeamId = t.TeamId, UserId = t.UserId, Temporary = true });

            Context.RemoveCacheEntry($"`c{cid}`teams`t{teamId}");
            Context.RemoveCacheEntry($"`c{cid}`teams`u{user.Id}");
            */
        }

        /*
        public async Task<List<(Team, string)>> BatchCreateAsync(
            Contest contest,
            Affiliation aff,
            Category cat,
            string[] names)
        {
            var rng = CreatePasswordGenerator();
            var result = new List<(Team, string)>();

            var list2 = await Context.Set<Team>()
                .Where(c => c.ContestId == contest.Id && c.AffiliationId == aff.Id && c.CategoryId == cat.Id)
                .ToListAsync();
            var list = list2.ToLookup(a => a.TeamName);

            foreach (var item2 in names)
            {
                var item = item2.Trim();

                if (list.Contains(item))
                {
                    var e = list[item];
                    foreach (var team in e)
                    {
                        string pwd = rng();
                        await EnsureTeamWithPassword(userManager, contest.Id, team.TeamId, pwd);
                        result.Add((team, pwd));
                    }
                }
                else
                {
                    var team = new Team
                    {
                        AffiliationId = aff.Id,
                        CategoryId = cat.Id,
                        ContestId = contest.Id,
                        Status = 1,
                        TeamName = item,
                    };

                    await CreateAsync(team, null);
                    string pwd = rng();
                    await EnsureTeamWithPassword(userManager, contest.Id, team.TeamId, pwd);
                    result.Add((team, pwd));
                }
            }

            return result;
        }

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
