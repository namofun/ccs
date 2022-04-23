using System;
using System.Collections.Generic;
using Xylab.Tenant.Entities;
using Xylab.Contesting.Scoreboard;

namespace Xylab.Contesting.Models
{
    public class ScoreboardModel
    {
        public IReadOnlyDictionary<int, IScoreboardRow> Data { get; }

        public IReadOnlyDictionary<int, Category> Categories { get; }

        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; }

        public ProblemCollection Problems { get; }

        public DateTimeOffset RefreshTime { get; }

        public int ContestId { get; }

        public IContestTime ContestTime { get; }

        public IRankingStrategy RankingStrategy { get; }

        public SortOrderLookup Public { get; }

        public SortOrderLookup Restricted { get; }

        public ScoreboardModel(
            int cid,
            IRankingStrategy rule,
            IContestTime time,
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
            ContestTime = time;
        }

        public ScoreboardModel(
            int cid,
            IReadOnlyDictionary<int, IScoreboardRow> data,
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations,
            ProblemCollection problems,
            IContestTime time,
            IRankingStrategy rankingStrategy)
            : this(
                  cid,
                  rankingStrategy,
                  time,
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
                0, null!, new TimeOnlyModel(),
                new Dictionary<int, IScoreboardRow>(),
                new Dictionary<int, Category>(),
                new Dictionary<int, Affiliation>(),
                new ProblemCollection(new List<ProblemModel>(), new Dictionary<int, (int, int)>()),
                SortOrderLookup.Empty,
                SortOrderLookup.Empty);
    }
}
