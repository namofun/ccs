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
    public class ContestRepository<TContext> : IContestRepository, ISupportDbContext, IContestQueryableStore
        where TContext : DbContext, IContestDbContext
    {
        public IContestDbContext Db { get; }

        public ContestRepository(TContext context)
            => Db = context;

        public Task<Contest?> FindAsync(int cid)
            => Db.Contests
                .Where(c => c.Id == cid)
                .SingleOrDefaultAsync()!;

        public Task UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression)
            => Db.Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(expression);

        private IQueryable<Problem2Model> QueryProblemsUsage(int probid)
            => from cp in Db.ContestProblems
               where cp.ProblemId == probid
               join c in Db.Contests on cp.ContestId equals c.Id
               select new Problem2Model(
                   cp.ContestId, cp.ProblemId, cp.ShortName,
                   cp.AllowSubmit, cp.Color, cp.Score,
                   c.Name, c.ShortName, c.Kind, c.RankingStrategy);

        private IQueryable<ParticipantModel> QueryParticipant(int userid)
            => from tm in Db.TeamMembers
               where tm.UserId == userid
               join t in Db.Teams on new { tm.ContestId, tm.TeamId } equals new { t.ContestId, t.TeamId }
               join c in Db.Contests on tm.ContestId equals c.Id
               join tc in Db.Categories on t.CategoryId equals tc.Id
               join ta in Db.Affiliations on t.AffiliationId equals ta.Id
               select new ParticipantModel(
                   c.Id, c.Name, t.TeamId, t.TeamName,
                   ta.Abbreviation, ta.Name, tc.Name, t.Status);

        public Task<List<Problem2Model>> FindProblemUsageAsync(int probid)
            => QueryProblemsUsage(probid).ToListAsync();

        public Task<List<ParticipantModel>> FindParticipantOfAsync(int userid)
            => QueryParticipant(userid).ToListAsync();

        public Task<IPagedList<ContestListModel>> ListAsync(int page = 1, int limit = 100)
            => Db.Contests
                .OrderByDescending(c => c.Id)
                .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTimeSeconds, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount))
                .ToPagedListAsync(page, limit);

        public Task<IPagedList<ContestListModel>> ListAsync(ClaimsPrincipal user, int? kind = null, int page = 1, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async Task<Contest> CreateAndAssignAsync(int kind, ClaimsPrincipal user)
        {
            var e = Db.Contests.Add(new Contest { Kind = kind });
            await Db.SaveChangesAsync();

            var uid = int.Parse(user.GetUserId()!);
            Db.ContestJuries.Add(new Jury { ContestId = e.Entity.Id, UserId = uid });
            await Db.SaveChangesAsync();

            return e.Entity;
        }

        async Task<ContestWrapper?> IContestRepository.FindAsync(int cid)
        {
            var entity = await FindAsync(cid);
            if (entity == null) return null;
            return new ContestWrapper(entity);
        }

        async Task<ContestWrapper> IContestRepository.CreateAndAssignAsync(int kind, ClaimsPrincipal user)
        {
            var entity = await CreateAndAssignAsync(kind, user);
            return new ContestWrapper(entity);
        }

        #region Queryable Store

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

        #endregion
    }
}
