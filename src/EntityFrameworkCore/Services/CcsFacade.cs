﻿using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : ICcsFacade
        where TContext : DbContext
        where TUser : SatelliteSite.IdentityModule.Entities.User
    {
        public IContestStore ContestStore => this;

        public IBalloonStore BalloonStore => this;

        public IClarificationStore ClarificationStore => this;

        public IProblemsetStore ProblemStore => this;

        public ITeamStore TeamStore => this;

        public TContext Context { get; }

        public DbSet<Balloon> Balloons => Context.Set<Balloon>();

        public DbSet<Team> Teams => Context.Set<Team>();

        public DbSet<Submission> Submissions => Context.Set<Submission>();

        public DbSet<Contest> Contests => Context.Set<Contest>();

        public DbSet<ContestProblem> ContestProblems => Context.Set<ContestProblem>();

        public DbSet<Problem> Problems => Context.Set<Problem>();

        public DbSet<Member> Members => Context.Set<Member>();

        public DbSet<Clarification> Clarifications => Context.Set<Clarification>();

        public DbSet<Jury> Juries => Context.Set<Jury>();

        public DbSet<TUser> Users => Context.Set<TUser>();

        public DbSet<Event> Events => Context.Set<Event>();

        public CcsFacade(TContext context)
        {
            Context = context;
        }
    }
}
