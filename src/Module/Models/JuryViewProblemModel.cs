using System.Collections;
using System.Collections.Generic;
using Xylab.Contesting.Models;
using Xylab.Polygon.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewProblemModel : IReadOnlyList<Solution>
    {
        private readonly IReadOnlyList<Solution> _list;
        private readonly ProblemModel _prob;

        public Solution this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<Solution> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public JuryViewProblemModel(IReadOnlyList<Solution> core, ProblemModel prob)
        {
            _list = core;
            _prob = prob;
        }

        public int Rank => _prob.Rank;
        public bool AllowJudge => _prob.AllowJudge;
        public string Title => _prob.Title;
        public int TimeLimit => _prob.TimeLimit;
        public int MemoryLimit => _prob.MemoryLimit;
        public int TestcaseCount => _prob.TestcaseCount;
        public bool Interactive => _prob.Interactive;
        public bool Shared => _prob.Shared;
        public int ContestId => _prob.ContestId;
        public int ProblemId => _prob.ProblemId;
        public string ShortName => _prob.ShortName;
        public bool AllowSubmit => _prob.AllowSubmit;
        public string Color => _prob.Color;
        public int Score => _prob.Score;
        public string Statement => _prob.Statement;
    }
}
