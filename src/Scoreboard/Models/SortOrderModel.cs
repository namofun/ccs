using System.Collections;
using System.Collections.Generic;

namespace Ccs.Models
{
    public class SortOrderModel : IReadOnlyList<TeamModel>
    {
        readonly IReadOnlyList<TeamModel> _inner;

        public ProblemStatisticsModel[]? Statistics { get; }

        public int Count => _inner.Count;

        public TeamModel this[int index] => _inner[index];

        public IEnumerator<TeamModel> GetEnumerator() => _inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        public SortOrderModel(
            IReadOnlyList<TeamModel> items,
            ProblemStatisticsModel[]? stats)
        {
            _inner = items;
            Statistics = stats;
        }
    }
}
