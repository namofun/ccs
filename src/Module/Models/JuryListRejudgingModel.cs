using System.Collections;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryListRejudgingModel : IReadOnlyList<Rejudging>
    {
        private readonly IReadOnlyList<Rejudging> _list;

        public Rejudging this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<Rejudging> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public JuryListRejudgingModel(IReadOnlyList<Rejudging> core) => _list = core;

        public IReadOnlyDictionary<int, string> UserNames { get; set; }
    }
}
