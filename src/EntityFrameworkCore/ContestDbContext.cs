using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Tenant.Entities;

namespace Ccs.Services
{
    public interface IContestDbContext
    {
        DbSet<Contest> Contests { get; set; }

        DbSet<ContestProblem> ContestProblems { get; set; }

        DbSet<Jury> Juries { get; set; }

        DbSet<Team> Teams { get; set; }

        DbSet<Member> TeamMembers { get; set; }

        DbSet<Clarification> Clarifications { get; set; }

        DbSet<Balloon> Balloons { get; set; }

        DbSet<Event> Events { get; set; }

        DbSet<Printing> Printings { get; set; }

        DbSet<RankCache> RankCache { get; set; }

        DbSet<ScoreCache> ScoreCache { get; set; }

        DbSet<Submission> Submissions { get; set; }

        DbSet<Judging> Judgings { get; set; }

        DbSet<JudgingRun> JudgingRuns { get; set; }

        DbSet<Testcase> Testcases { get; set; }

        DbSet<Category> Categories { get; set; }
    }
}
