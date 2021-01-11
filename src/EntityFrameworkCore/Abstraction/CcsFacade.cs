using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public interface ICcsFacade
    {
        IContestStore ContestStore { get; }

        IBalloonStore BalloonStore { get; }

        DbSet<Clarification> Clarifications { get; }

        DbSet<Team> Teams { get; }

        DbSet<Member> Members { get; }

        IQueryable<IUser> Users { get; }

        Task<int> SaveChangesAsync();

        IProblemsetStore ProblemStore { get; }

        ITeamStore TeamStore { get; }
    }
}
