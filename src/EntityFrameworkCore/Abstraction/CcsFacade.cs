using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public interface ICcsFacade
    {
        IContestStore ContestStore { get; }

        DbSet<Contest> Contests { get; }

        DbSet<ContestProblem> ContestProblems { get; }

        DbSet<Problem> Problems { get; }

        DbSet<ProblemAuthor> ProblemAuthors { get; }

        DbSet<Testcase> Testcases { get; }

        DbSet<Clarification> Clarifications { get; }

        DbSet<Team> Teams { get; }

        DbSet<Member> Members { get; }

        DbSet<Balloon> Balloons { get; }

        IQueryable<IUser> Users { get; }

        Task<int> SaveChangesAsync();

        DbSet<Submission> Submissions { get; }
    }
}
