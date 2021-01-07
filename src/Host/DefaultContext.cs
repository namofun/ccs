using Ccs.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, Role, int>
    {
        public DefaultContext(DbContextOptions options)
            : base(options)
        {
        }
    }

    public class QueryCache : QueryCacheBase<DefaultContext>
    {
        public override Task<IEnumerable<(int UserId, string UserName, string NickName)>> FetchPermittedUserAsync(DefaultContext context, int probid)
        {
            throw new NotImplementedException();
        }

        public override Task<List<SolutionAuthor>> FetchSolutionAuthorAsync(DefaultContext context, Expression<Func<Submission, bool>> predicate)
        {
            var query =
                from s in context.Set<Submission>().WhereIf(predicate != null, predicate)
                join u in context.Set<User>() on new { s.ContestId, s.TeamId } equals new { ContestId = 0, TeamId = u.Id }
                into uu from u in uu.DefaultIfEmpty()
                join t in context.Set<Team>() on new { s.ContestId, s.TeamId } equals new { t.ContestId, t.TeamId }
                into tt from t in tt.DefaultIfEmpty()
                select new SolutionAuthor(s.Id, s.ContestId, s.TeamId, u.UserName, t.TeamName);
            return query.ToListAsync();
        }

        protected override Expression<Func<DateTimeOffset, DateTimeOffset, double>> CalculateDuration { get; }
            = (start, end) => EF.Functions.DateDiffMillisecond(start, end) / 1000.0;
    }
}
