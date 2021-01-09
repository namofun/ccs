namespace Ccs.Services
{
    public interface ICcsFacade
    {
        IContestStore ContestStore { get; }

        IBalloonStore BalloonStore { get; }

        IClarificationStore ClarificationStore { get; }

        IProblemsetStore ProblemStore { get; }

        ITeamStore TeamStore { get; }
    }
}
