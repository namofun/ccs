using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ccs.Models
{
    public class SortOrderLookup : ILookup<int, IScoreboardRow>
    {
        private readonly IReadOnlyDictionary<int, SortOrder> _sortOrders;

        public static SortOrderLookup Empty { get; } = new SortOrderLookup();

        public IEnumerable<IScoreboardRow> this[int key]
            => _sortOrders.TryGetValue(key, out var so)
                ? so
                : new SortOrder(Array.Empty<IScoreboardRow>(), key);

        public int Count
            => _sortOrders.Count;

        public bool Contains(int key)
            => _sortOrders.ContainsKey(key);

        public IEnumerator<IGrouping<int, IScoreboardRow>> GetEnumerator()
            => _sortOrders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private SortOrderLookup()
            => _sortOrders = new Dictionary<int, SortOrder>();

        public SortOrderLookup(
            IEnumerable<IGrouping<int, IScoreboardRow>> rows,
            bool isPublic,
            Scoreboard.IRankingStrategy sort)
            => _sortOrders = rows
                .Select(g => new SortOrder(sort.SortByRule(g, isPublic).ToList(), g.Key))
                .ToDictionary(s => s.Key);

        public SortOrderLookup(
            IReadOnlyDictionary<int, Tenant.Entities.Category> categories,
            IReadOnlyDictionary<int, IScoreboardRow> rows,
            bool isPublic,
            Scoreboard.IRankingStrategy sort)
            : this(rows.Values
                        .Where(r => categories.ContainsKey(r.CategoryId))
                        .GroupBy(r => categories[r.CategoryId].SortOrder),
                  isPublic,
                  sort)
        {
        }

        private class SortOrder :
            IGrouping<int, IScoreboardRow>,
            IReadOnlyList<IScoreboardRow>
        {
            private readonly IReadOnlyList<IScoreboardRow> _rows;

            public IScoreboardRow this[int index] => _rows[index];

            public int Count => _rows.Count;

            public int Key { get; }

            public IEnumerator<IScoreboardRow> GetEnumerator() => _rows.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _rows.GetEnumerator();

            public SortOrder(IReadOnlyList<IScoreboardRow> rows, int order) => (_rows, Key) = (rows, order);
        }
    }
}
