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

        public DateTimeOffset RefreshTime { get; }

        public SortOrderLookup Public { get; }

        public SortOrderLookup Restricted { get; }

        public ScoreboardModel(
            IReadOnlyDictionary<int, IScoreboardRow> data,
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations,
            SortOrderLookup @public,
            SortOrderLookup restricted)
        {
            Data = data;
            RefreshTime = DateTimeOffset.Now;
            Categories = categories;
            Affiliations = affiliations;
            Public = @public;
            Restricted = restricted;
        }

        public ScoreboardModel(
            IReadOnlyDictionary<int, IScoreboardRow> data,
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations,
            Scoreboard.IRankingStrategy rankingStrategy)
            : this(data,
                  categories,
                  affiliations,
                  new SortOrderLookup(categories, data, true, rankingStrategy),
                  new SortOrderLookup(categories, data, false, rankingStrategy))
        {
        }

        public static ScoreboardModel Empty { get; }
            = new ScoreboardModel(
                new Dictionary<int, IScoreboardRow>(),
                new Dictionary<int, Category>(),
                new Dictionary<int, Affiliation>(),
                SortOrderLookup.Empty,
                SortOrderLookup.Empty);
    }
}
