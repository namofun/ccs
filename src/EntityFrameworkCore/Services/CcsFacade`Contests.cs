using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : IContestStore
    {
        Task<Contest> IContestStore.FindAsync(int cid)
        {
            return Contests
                .Where(c => c.Id == cid)
                .SingleOrDefaultAsync();
        }

        Task<int> IContestStore.MaxEventIdAsync(int cid)
        {
            return Context.Set<Event>()
                .Where(e => e.ContestId == cid)
                .OrderByDescending(e => e.Id)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        Task IContestStore.UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression)
        {
            return Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(expression);
        }

        Task<List<Event>> IContestStore.FetchEventAsync(int cid, string[]? type, int after)
        {
            return Events
                .Where(e => e.ContestId == cid && e.Id > after)
                .WhereIf(type != null, e => type.Contains(e.EndpointType))
                .ToListAsync();
        }

        Task IContestStore.EmitAsync(Event @event)
        {
            Events.Add(@event);
            return Context.SaveChangesAsync();
        }

        Task IContestStore.AssignJuryAsync(int cid, int userid)
        {
            return Juries.UpsertAsync(
                new { cid, userid },
                s => new Jury { ContestId = cid, UserId = userid });
        }

        Task IContestStore.UnassignJuryAsync(int cid, int userid)
        {
            return Juries
                .Where(j => j.ContestId == cid && j.UserId == userid)
                .BatchDeleteAsync();
        }

        async Task<HashSet<int>> IContestStore.ListJuryAsync(int cid)
        {
            var result = new HashSet<int>();
            var query = await Juries
                .Where(j => j.ContestId == cid)
                .Select(j => j.UserId)
                .ToListAsync();
            return new HashSet<int>(query);
        }

        Task<IPagedList<ContestListModel>> IContestRepository.ListAsync(int userId, int kind, int page, int limit)
        {
            throw new NotImplementedException();
        }

        Task<IPagedList<ContestListModel>> IContestRepository.ListAsync(int page, int limit)
        {
            return Contests
                .OrderByDescending(c => c.Id)
                .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTime, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount))
                .ToPagedListAsync(page, limit);
        }

        async Task<Contest> IContestRepository.CreateAndAssignAsync(int kind, ClaimsPrincipal user)
        {
            var e = Contests.Add(new Contest { Kind = kind });
            await Context.SaveChangesAsync();

            var uid = int.Parse(user.GetUserId());
            Juries.Add(new Jury { ContestId = e.Entity.Id, UserId = uid });
            await Context.SaveChangesAsync();

            return e.Entity;
        }

        Task<List<Problem2Model>> IContestRepository.FindProblemUsageAsync(int probid)
        {
            var query =
                from cp in ContestProblems
                where cp.ProblemId == probid
                join c in Contests on cp.ContestId equals c.Id
                select new Problem2Model(
                    cp.ContestId,
                    cp.ProblemId,
                    cp.ShortName,
                    cp.AllowSubmit,
                    cp.Color,
                    cp.Score,
                    c.Name,
                    c.Kind,
                    c.RankingStrategy);

            return query.ToListAsync();
        }
    }
}
