using Polygon.Events;

namespace Ccs.Scoreboard
{
    internal class JudgingFinishedEvent2 : JudgingFinishedEvent
    {
        public int CodeforcesScore { get; }

        public JudgingFinishedEvent2(JudgingFinishedEvent e, int cfscore) :
            base(e.Judging, e.ContestId, e.ProblemId, e.TeamId, e.SubmitTime)
        {
            CodeforcesScore = cfscore;
        }
    }
}
