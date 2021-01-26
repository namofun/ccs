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
    public class ContestRepository<TContext> : IContestRepository, ISupportDbContext
        where TContext : DbContext, IContestDbContext
    {
        public IContestDbContext Db { get; }

        public ContestRepository(TContext context)
            => Db = context;

        public Task<Contest> FindAsync(int cid)
            => Db.Contests
                .Where(c => c.Id == cid)
                .SingleOrDefaultAsync();

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
                .Select(c => new ContestListModel(c.Id, c.Name, c.ShortName, c.StartTime, c.EndTime, c.Kind, c.RankingStrategy, c.IsPublic, c.TeamCount, c.ProblemCount))
                .ToPagedListAsync(page, limit);

        public Task<IPagedList<ContestListModel>> ListAsync(int userId, int kind, int page = 1, int limit = 100)
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
    }
}
