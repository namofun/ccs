using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : IBalloonStore
    {
        Task IBalloonStore.SetDoneAsync(Contest contest, int id)
        {
            int cid = contest.Id;
            return Balloons.BatchUpdateJoinAsync(
                inner: Submissions,
                outerKeySelector: b => b.SubmissionId,
                innerKeySelector: s => s.Id,
                updateSelector: (b, _) => new Balloon { Done = true },
                condition: (b, s) => b.Id == id && s.ContestId == cid);
        }

        Task IBalloonStore.CreateAsync(int submissionId)
        {
            Balloons.Add(new Balloon { SubmissionId = submissionId });
            return Context.SaveChangesAsync();
        }

        async Task<List<BalloonModel>> IBalloonStore.ListAsync(Contest contest, IReadOnlyList<ProblemModel> probs)
        {
            int cid = contest.Id;
            var balloonQuery =
                from b in Balloons
                join s in Submissions on b.SubmissionId equals s.Id
                where s.ContestId == cid
                orderby s.Time
                join t in Teams
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
                var p = probs.FirstOrDefault(p => p.ProblemId == item.ProblemId);
                if (p == null) continue;
                item.FirstToSolve = hashSet.Add(((long)item.ProblemId) << 32 | (long)item.SortOrder);
                item.BalloonColor = p.Color;
                item.ProblemShortName = p.ShortName;
            }

            return balloons;
        }
    }
}
