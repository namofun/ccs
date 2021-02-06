using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IBalloonContext
    {
        public virtual async Task<List<BalloonModel>> FetchBalloonsAsync()
        {
            var problems = await FetchProblemsAsync();
            int cid = Contest.Id;
            var balloonQuery =
                from b in Db.Balloons
                join s in Db.Submissions on b.SubmissionId equals s.Id
                where s.ContestId == cid
                orderby s.Time
                join t in Db.Teams
                    on new { s.ContestId, s.TeamId }
                    equals new { t.ContestId, t.TeamId }
                select new BalloonModel(
                    b.Id,
                    b.SubmissionId,
                    b.Done,
                    s.ProblemId,
                    s.TeamId,
                    t.TeamName,
                    t.Location,
                    s.Time,
                    t.Category.Name,
                    t.Category.SortOrder);

            var balloons = new List<BalloonModel>();
            var hashSet = new HashSet<long>();
            await foreach (var item in balloonQuery.AsAsyncEnumerable())
            {
                var p = problems.Find(item.ProblemId);
                if (p == null) continue;
                item.FirstToSolve = hashSet.Add(((long)item.ProblemId) << 32 | (long)item.SortOrder);
                item.BalloonColor = p.Color;
                item.ProblemShortName = p.ShortName;
            }

            return balloons;
        }

        public virtual Task SetBalloonDoneAsync(int id)
        {
            int cid = Contest.Id;
            return Db.Balloons.BatchUpdateJoinAsync(
                inner: Db.Submissions,
                outerKeySelector: b => b.SubmissionId,
                innerKeySelector: s => s.Id,
                updateSelector: (b, _) => new Balloon { Done = true },
                condition: (b, s) => b.Id == id && s.ContestId == cid);
        }
    }
}
