using Xylab.Contesting.Entities;
using System.Linq;

namespace Xylab.Contesting.Services
{
    public partial class ImmediateContestContext :
        Xylab.Contesting.Services.IAnalysisContext,
        Xylab.Contesting.Services.IBalloonContext,
        Xylab.Contesting.Services.IClarificationContext,
        Xylab.Contesting.Services.ICompeteContext,
        Xylab.Contesting.Services.IContestContext,
        Xylab.Contesting.Services.IContestQueryableStore,
        Xylab.Contesting.Services.IDomContext,
        Xylab.Contesting.Services.IGymContext,
        Xylab.Contesting.Services.IJuryContext,
        Xylab.Contesting.Services.IProblemContext,
        Xylab.Contesting.Services.IProblemsetContext,
        Xylab.Contesting.Services.IRejudgingContext,
        Xylab.Contesting.Services.ISubmissionContext,
        Xylab.Contesting.Services.ITeamContext
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
        IQueryable<Visibility> IContestQueryableStore.ContestTenants => Db.ContestTenants;
    }
}
