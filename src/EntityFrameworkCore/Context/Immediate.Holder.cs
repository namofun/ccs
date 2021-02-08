using Ccs.Entities;
using System.Linq;

namespace Ccs.Services
{
    public partial class ImmediateContestContext :
        Ccs.Services.IAnalysisContext,
        Ccs.Services.IBalloonContext,
        Ccs.Services.IClarificationContext,
        Ccs.Services.ICompeteContext,
        Ccs.Services.IContestContext,
        Ccs.Services.IContestQueryableStore,
        Ccs.Services.IDomContext,
        Ccs.Services.IGymContext,
        Ccs.Services.IJuryContext,
        Ccs.Services.IProblemContext,
        Ccs.Services.IProblemsetContext,
        Ccs.Services.IRejudgingContext,
        Ccs.Services.ISubmissionContext,
        Ccs.Services.ITeamContext
    {
        IQueryable<Contest> IContestQueryableStore.Contests => Db.Contests;
        IQueryable<ContestProblem> IContestQueryableStore.ContestProblems => Db.ContestProblems;
        IQueryable<Jury> IContestQueryableStore.ContestJuries => Db.ContestJuries;
        IQueryable<Team> IContestQueryableStore.Teams => Db.Teams;
        IQueryable<Member> IContestQueryableStore.TeamMembers => Db.TeamMembers;
        IQueryable<Clarification> IContestQueryableStore.Clarifications => Db.Clarifications;
        IQueryable<Balloon> IContestQueryableStore.Balloons => Db.Balloons;
        IQueryable<Event> IContestQueryableStore.ContestEvents => Db.ContestEvents;
        IQueryable<Printing> IContestQueryableStore.Printings => Db.Printings;
        IQueryable<RankCache> IContestQueryableStore.RankCache => Db.RankCache;
        IQueryable<ScoreCache> IContestQueryableStore.ScoreCache => Db.ScoreCache;
    }
}
