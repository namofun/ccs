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
        async Task<List<BalloonModel>> IBalloonContext.ListAsync()
        {
            var problems = await ListProblemsAsync();
            int cid = Contest.Id;
            var balloonQuery =
                from b in Db.Balloons
                join s in Db.Submissions on b.SubmissionId equals s.Id
                where s.ContestId == cid
                orderby s.Time
                join t in Db.Teams on new { s.ContestId, s.TeamId } equals new { t.ContestId, t.TeamId }
                join c in Db.Categories on t.CategoryId equals c.Id
                select new BalloonModel(
                    b.Id,
                    b.SubmissionId,
                    b.Done,
                    s.ProblemId,
                    s.TeamId,
                    t.TeamName,
                    t.Location,
                    s.Time,
                    c.Name,
                    c.SortOrder);

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

        Task IBalloonContext.HandleAsync(int id)
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
