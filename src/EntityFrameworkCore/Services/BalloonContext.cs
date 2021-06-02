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
            int cid = Contest.Id;
            var problems = await ListProblemsAsync();
            var categories = await ListCategoriesAsync(true);
            var affiliations = await ListAffiliationsAsync(true);
            var scb = await GetScoreboardAsync();

            var balloonQuery =
                from b in Db.Balloons
                join s in Db.Submissions on b.SubmissionId equals s.Id
                where s.ContestId == cid
                orderby s.Time
                select new BalloonModel(
                    b.Id,
                    b.SubmissionId,
                    b.Done,
                    s.ProblemId,
                    s.TeamId,
                    s.Time);

            var balloons = new List<BalloonModel>();
            var dones = new List<BalloonModel>();
            var hashSet = new HashSet<(int, int)>();
            var prev = new Dictionary<int, BalloonModel>();
            await foreach (var item in balloonQuery.AsAsyncEnumerable())
            {
                var p = problems.Find(item.ProblemId);
                if (p == null) continue;
                if (!scb.Data.TryGetValue(item.TeamId, out var team)) continue;
                if (!categories.TryGetValue(team.CategoryId, out var cat)) continue;
                if (!affiliations.TryGetValue(team.AffiliationId, out var aff)) continue;

                item.BalloonColor = p.Color;
                item.ProblemShortName = p.ShortName;
                item.CategoryName = cat.Name;
                item.SortOrder = cat.SortOrder;
                item.TeamName = $"t{team.TeamId}: {team.TeamName}";
                item.AffiliationName = aff.Name;
                item.AffiliationShortName = aff.Abbreviation;
                item.Location = team.TeamLocation ?? string.Empty;
                item.Previous = prev.GetValueOrDefault(item.TeamId);
                item.FirstToSolve = hashSet.Add((item.ProblemId, item.SortOrder));
                prev[item.TeamId] = item;
                (item.Done ? dones : balloons).Add(item);
            }

            dones.Reverse();
            balloons.AddRange(dones);
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
