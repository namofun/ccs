using Polygon.Models;
using System.Collections;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Components.SolutionList
{
    public class SolutionListViewModel : IReadOnlyList<Solution>
    {
        private readonly IReadOnlyList<Solution> _solutions;

        public Solution this[int index] => _solutions[index];

        public int Count => _solutions.Count;

        public IEnumerator<Solution> GetEnumerator() => _solutions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public SolutionListViewModel(IReadOnlyList<Solution> list) => _solutions = list;

        public bool ShowTeams { get; set; }

        public bool ShowProblems { get; set; }

        public bool ShowIp { get; set; }
    }
}
