using Ccs.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ccs
{
    /// <summary>
    /// Represents a read-only collection of contest problems.
    /// </summary>
    public class ProblemCollection : IReadOnlyList<ProblemModel>
    {
        private readonly List<ProblemModel> _problems;

        /// <summary>
        /// Creates a collection for problems.
        /// </summary>
        /// <param name="problems">The problem models.</param>
        /// <param name="testcases">The testcase informations.</param>
        public ProblemCollection(List<ProblemModel> problems, Dictionary<int, (int, int)> testcases)
        {
            problems.Sort((a, b) => a.ShortName.CompareTo(b.ShortName));
            for (int i = 0; i < problems.Count; i++)
            {
                problems[i].Rank = i + 1;
                if (testcases.TryGetValue(problems[i].ProblemId, out var res))
                {
                    problems[i].TestcaseCount = res.Item1;
                    if (problems[i].Score == 0) problems[i].Score = res.Item2;
                }
            }

            _problems = problems;
        }

        /// <summary>
        /// Find the contest problem with corresponding problem ID.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public ProblemModel? Find(int probid)
        {
            return _problems.FirstOrDefault(p => p.ProblemId == probid);
        }

        /// <summary>
        /// Find the contest problem with corresponding problem short name.
        /// </summary>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public ProblemModel? Find(string shortName)
        {
            return _problems.FirstOrDefault(p => p.ShortName == shortName);
        }

        /// <inheritdoc />
        public ProblemModel this[int index] => _problems[index];

        /// <inheritdoc />
        public int Count => _problems.Count;

        /// <inheritdoc />
        public IEnumerator<ProblemModel> GetEnumerator() => _problems.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
