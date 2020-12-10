using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class BalloonStore<TContext> : IBalloonStore
        where TContext : DbContext
    {
        public TContext Context { get; }

        public DbSet<Balloon> Balloons => Context.Set<Balloon>();

        public BalloonStore(TContext context)
        {
            Context = context;
        }

        public Task SetDoneAsync(Contest contest, int id)
        {
            int cid = contest.Id;
            return Balloons
                .Where(b => b.Id == id && b.Submission.ContestId == cid)
                .BatchUpdateAsync(b => new Balloon { Done = true });
        }

        public Task CreateAsync(int submissionId)
        {
            Balloons.Add(new Balloon { SubmissionId = submissionId });
            return Context.SaveChangesAsync();
        }

        public async Task<List<BalloonModel>> ListAsync(Contest contest, Dictionary<int, ProblemModel> probs)
        {
            int cid = contest.Id;
            var balloonQuery =
                from b in Balloons
                where b.Submission.ContestId == cid
                join t in Context.Set<Team>()
                    on new { b.Submission.ContestId, b.Submission.TeamId }
                    equals new { t.ContestId, t.TeamId }
                orderby b.Submission.Time
                select new BalloonModel(
                    b.Id,
                    b.SubmissionId,
                    b.Done,
                    b.Submission.ProblemId,
                    b.Submission.TeamId,
                    t.TeamName,
                    t.Location,
                    b.Submission.Time,
                    t.Category.Name,
                    t.Category.SortOrder);

            var balloons = new List<BalloonModel>();
            var hashSet = new HashSet<long>();
            await foreach (var item in balloonQuery.AsAsyncEnumerable())
            {
                if (!probs.TryGetValue(item.ProblemId, out var p)) continue;
                item.FirstToSolve = hashSet.Add(((long)item.ProblemId) << 32 | (long)item.SortOrder);
                item.BalloonColor = p.Color;
                item.ProblemShortName = p.ShortName;
            }

            return balloons;
        }
    }
}
