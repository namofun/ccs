using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    /// <summary>
    /// The marker interface to gets the <see cref="DbSet{TEntity}"/>s.
    /// </summary>
    public interface IContestDbContext
    {
        /// <summary>
        /// Gets the database set for <see cref="Contest"/>.
        /// </summary>
        DbSet<Contest> Contests { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="ContestProblem"/>.
        /// </summary>
        DbSet<ContestProblem> ContestProblems { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Jury"/>.
        /// </summary>
        DbSet<Jury> ContestJuries { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Team"/>.
        /// </summary>
        DbSet<Team> Teams { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Member"/>.
        /// </summary>
        DbSet<Member> TeamMembers { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Clarification"/>.
        /// </summary>
        DbSet<Clarification> Clarifications { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Balloon"/>.
        /// </summary>
        DbSet<Balloon> Balloons { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Event"/>.
        /// </summary>
        DbSet<Event> ContestEvents { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Printing"/>.
        /// </summary>
        DbSet<Printing> Printings { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Entities.RankCache"/>.
        /// </summary>
        DbSet<RankCache> RankCache { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Entities.ScoreCache"/>.
        /// </summary>
        DbSet<ScoreCache> ScoreCache { get; set; }

        /// <summary>
        /// Gets the database set for <see cref="Visibility"/>.
        /// </summary>
        DbSet<Visibility> ContestTenants { get; set; }

        /// <summary>
        /// Gets the queryable for <see cref="Submission"/>.
        /// </summary>
        IQueryable<Submission> Submissions { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Polygon.Entities.SubmissionStatistics"/>.
        /// </summary>
        IQueryable<SubmissionStatistics> SubmissionStatistics { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Problem"/>.
        /// </summary>
        IQueryable<Problem> Problems { get; }

        /// <summary>
        /// Gets the queryable for <see cref="ProblemAuthor"/>.
        /// </summary>
        IQueryable<ProblemAuthor> ProblemAuthors { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Judging"/>.
        /// </summary>
        IQueryable<Judging> Judgings { get; }

        /// <summary>
        /// Gets the queryable for <see cref="JudgingRun"/>.
        /// </summary>
        IQueryable<JudgingRun> JudgingRuns { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Testcase"/>.
        /// </summary>
        IQueryable<Testcase> Testcases { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Affiliation"/>.
        /// </summary>
        IQueryable<Affiliation> Affiliations { get; }

        /// <summary>
        /// Gets the queryable for <see cref="Category"/>.
        /// </summary>
        IQueryable<Category> Categories { get; }

        /// <summary>
        /// Gets the queryable for <see cref="IUser"/>.
        /// </summary>
        IQueryable<IUser> Users { get; }

        /// <summary>
        /// <para>
        /// Saves all changes made in this context to the database.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not supported. Use
        /// 'await' to ensure that any asynchronous operations have completed before calling
        /// another method on this context.
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        /// <exception cref="DbUpdateException">An error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">A concurrency violation is encountered while saving to the database. A concurrency violation occurs when an unexpected number of rows are affected during save. This is usually because the data in the database has been modified since it was loaded into memory.</exception>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
