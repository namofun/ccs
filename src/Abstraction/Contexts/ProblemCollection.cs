using Ccs.Entities;
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
            for (int i = 0; i < _problems.Count; i++)
                if (_problems[i].ProblemId == probid)
                    return _problems[i];
            return null;
        }

        /// <summary>
        /// Find the contest problem with corresponding problem short name.
        /// </summary>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public ProblemModel? Find(string shortName)
        {
            for (int i = 0; i < _problems.Count; i++)
                if (_problems[i].ShortName == shortName)
                    return _problems[i];
            return null;
        }

        /// <inheritdoc />
        public ProblemModel this[int index] => _problems[index];

        /// <summary>
        /// Find the contest problem with corresponding problem short name.
        /// </summary>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The problem model or <c>null</c>.</returns>
        public ProblemModel this[string shortName]
            => Find(shortName) ?? throw new KeyNotFoundException();

        /// <inheritdoc />
        public int Count => _problems.Count;

        /// <inheritdoc />
        public IEnumerator<ProblemModel> GetEnumerator() => _problems.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the clarification categories from contest problems.
        /// </summary>
        /// <returns>The enumerable for tuple (CategoryName, CategoryEnum, ProblemId).</returns>
        public IEnumerable<(string, ClarificationCategory, int?, string)> ClarificationCategories
            => _problems
                .Select(cp => ($"prob-{cp.ShortName}", ClarificationCategory.Problem, (int?) cp.ProblemId, $"{cp.ShortName} - {cp.Title}"))
                .Prepend(("tech", ClarificationCategory.Technical, null, "Technical issue"))
                .Prepend(("general", ClarificationCategory.General, null, "General issue"));
    }
}
