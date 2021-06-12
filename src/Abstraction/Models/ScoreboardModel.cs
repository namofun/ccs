using System;
using System.Collections.Generic;
using Tenant.Entities;

namespace Ccs.Models
{
    public class ScoreboardModel
    {
        public IReadOnlyDictionary<int, IScoreboardRow> Data { get; }

        public IReadOnlyDictionary<int, Category> Categories { get; }

        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; }

        public ProblemCollection Problems { get; }

        public DateTimeOffset RefreshTime { get; }

        public int ContestId { get; }

        public int RankingStrategy { get; }

        public SortOrderLookup Public { get; }

        public SortOrderLookup Restricted { get; }

        public ScoreboardModel(
            int cid,
            int rule,
            IReadOnlyDictionary<int, IScoreboardRow> data,
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations,
            ProblemCollection problems,
            SortOrderLookup @public,
            SortOrderLookup restricted)
        {
            ContestId = cid;
            RankingStrategy = rule;
            Data = data;
            RefreshTime = DateTimeOffset.Now;
            Categories = categories;
            Affiliations = affiliations;
            Public = @public;
            Restricted = restricted;
            Problems = problems;
        }

        public ScoreboardModel(
            int cid,
            IReadOnlyDictionary<int, IScoreboardRow> data,
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations,
            ProblemCollection problems,
            Scoreboard.IRankingStrategy rankingStrategy)
            : this(
                  cid,
                  rankingStrategy.Id,
                  data,
                  categories,
                  affiliations,
                  problems,
                  new SortOrderLookup(categories, data, true, rankingStrategy),
                  new SortOrderLookup(categories, data, false, rankingStrategy))
        {
        }

        public static ScoreboardModel Empty { get; }
            = new ScoreboardModel(
                0, 0,
                new Dictionary<int, IScoreboardRow>(),
                new Dictionary<int, Category>(),
                new Dictionary<int, Affiliation>(),
                new ProblemCollection(new List<ProblemModel>(), new Dictionary<int, (int, int)>()),
                SortOrderLookup.Empty,
                SortOrderLookup.Empty);
    }
}
