using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polygon.Entities;
using SatelliteSite.IdentityModule.Entities;
using Tenant.Entities;

namespace Ccs.Entities
{
    public class ContestEntityConfiguration<TUser, TRole, TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Balloon>,
        IEntityTypeConfiguration<Clarification>,
        IEntityTypeConfiguration<Event>,
        IEntityTypeConfiguration<Printing>,
        IEntityTypeConfiguration<ScoreCache>,
        IEntityTypeConfiguration<RankCache>,
        IEntityTypeConfiguration<Team>,
        IEntityTypeConfiguration<Contest>,
        IEntityTypeConfiguration<Member>,
        IEntityTypeConfiguration<ContestProblem>,
        IEntityTypeConfiguration<Rejudging>,
        IEntityTypeConfiguration<Jury>
        where TUser : User
        where TRole : Role
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Balloon> entity)
        {
            entity.ToTable("ContestBalloons");

            entity.HasKey(e => e.Id);

            entity.HasOne<Submission>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Done)
                .HasDefaultValue(false);
        }

        public void Configure(EntityTypeBuilder<Clarification> entity)
        {
            entity.ToTable("ContestClarifications");

            entity.HasKey(e => e.Id);

            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Clarification>()
                .WithMany()
                .HasForeignKey(e => e.ResponseToId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ResponseToId);

            entity.HasOne<Team>()
                .WithMany()
                .HasForeignKey(e => new { e.ContestId, TeamId = e.Sender })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ContestId, TeamId = e.Recipient });

            entity.HasOne<Team>()
                .WithMany()
                .HasForeignKey(e => new { e.ContestId, TeamId = e.Recipient })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Problem>()
                .WithMany()
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Body)
                .IsRequired();
        }

        public void Configure(EntityTypeBuilder<Printing> entity)
        {
            entity.ToTable("ContestPrintings");

            entity.HasKey(e => e.Id);

            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SourceCode)
                .IsRequired()
                .HasMaxLength(65536);

            entity.Property(e => e.FileName)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.Property(e => e.LanguageId)
                .IsUnicode(false)
                .HasMaxLength(10);
        }

        public void Configure(EntityTypeBuilder<RankCache> entity)
        {
            entity.ToTable("ContestRankCache");

            entity.HasKey(e => new { e.ContestId, e.TeamId });

            entity.Property(e => e.PointsPublic)
                .HasDefaultValue(0);

            entity.Property(e => e.PointsRestricted)
                .HasDefaultValue(0);

            entity.Property(e => e.TotalTimeRestricted)
                .HasDefaultValue(0);

            entity.Property(e => e.TotalTimePublic)
                .HasDefaultValue(0);
        }

        public void Configure(EntityTypeBuilder<ScoreCache> entity)
        {
            entity.ToTable("ContestScoreCache");

            entity.HasKey(e => new { e.ContestId, e.TeamId, e.ProblemId });

            entity.Property(e => e.FirstToSolve)
                .HasDefaultValue(false);

            entity.Property(e => e.IsCorrectPublic)
                .HasDefaultValue(false);

            entity.Property(e => e.IsCorrectRestricted)
                .HasDefaultValue(false);

            entity.Property(e => e.PendingPublic)
                .HasDefaultValue(0);

            entity.Property(e => e.PendingRestricted)
                .HasDefaultValue(0);

            entity.Property(e => e.SubmissionPublic)
                .HasDefaultValue(0);

            entity.Property(e => e.SubmissionRestricted)
                .HasDefaultValue(0);
        }

        public void Configure(EntityTypeBuilder<Event> entity)
        {
            entity.ToTable("ContestEvents");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.EventTime);

            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EndpointType)
                .HasMaxLength(32)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.EndpointId)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.Action)
                .HasMaxLength(6)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.Content)
                .HasMaxLength(2048)
                .IsRequired();
        }

        public void Configure(EntityTypeBuilder<Team> entity)
        {
            entity.ToTable("ContestTeams");

            entity.HasKey(e => new { e.ContestId, e.TeamId });

            entity.HasOne<Contest>(e => e.Contest)
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TeamName)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne<Affiliation>(e => e.Affiliation)
                .WithMany()
                .HasForeignKey(e => e.AffiliationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Category>(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany<ScoreCache>(e => e.ScoreCache)
                .WithOne()
                .HasForeignKey(sc => new { sc.ContestId, sc.TeamId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<RankCache>(e => e.RankCache!)
                .WithOne()
                .HasForeignKey<RankCache>(rc => new { rc.ContestId, rc.TeamId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Location)
                .HasMaxLength(16)
                .IsRequired(false);

            entity.HasIndex(e => new { e.ContestId, e.Status });
        }

        public void Configure(EntityTypeBuilder<Contest> entity)
        {
            entity.ToTable("Contests");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.ShortName)
                .IsRequired();

            entity.Property(e => e.EndTimeSeconds)
                .HasColumnName(nameof(IContestInformation.EndTime));

            entity.Property(e => e.FreezeTimeSeconds)
                .HasColumnName(nameof(IContestInformation.FreezeTime));

            entity.Property(e => e.UnfreezeTimeSeconds)
                .HasColumnName(nameof(IContestInformation.UnfreezeTime));
        }

        public void Configure(EntityTypeBuilder<Member> entity)
        {
            entity.ToTable("ContestMembers");

            entity.HasKey(e => new { e.ContestId, e.TeamId, e.UserId });

            entity.HasOne<Team>(e => e.Team)
                .WithMany(e => e.Members)
                .HasForeignKey(e => new { e.ContestId, e.TeamId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ContestId, e.UserId })
                .IsUnique();
        }

        public void Configure(EntityTypeBuilder<ContestProblem> entity)
        {
            entity.ToTable("ContestProblems");

            entity.HasKey(e => new { e.ContestId, e.ProblemId });

            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Problem>()
                .WithMany()
                .HasForeignKey(e => e.ProblemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasIndex(e => new { e.ContestId, e.ShortName })
                .IsUnique();

            entity.Property(e => e.Color)
                .IsRequired();
        }

        public void Configure(EntityTypeBuilder<Jury> entity)
        {
            entity.ToTable("ContestJury");

            entity.HasKey(e => new { e.ContestId, e.UserId });

            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<TUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<Rejudging> entity)
        {
            entity.HasOne<Contest>()
                .WithMany()
                .HasForeignKey(e => e.ContestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
