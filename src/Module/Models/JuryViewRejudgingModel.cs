using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewRejudgingModel
    {
        private int? _undoneCountLazy;

        public IReadOnlyDictionary<int, string> TeamNames { get; set; }

        public IEnumerable<RejudgingDifference> Differences { get; set; }

        public int Id { get; set; }

        public string Reason { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public DateTimeOffset? EndTime { get; set; }

        public string IssuedBy { get; set; }

        public string OperatedBy { get; set; }

        public bool? Applied { get; set; }

        public int UndoneCount
            => _undoneCountLazy ??= Differences
                .Select(a => a.NewJudging)
                .Where(a => a.Status == Verdict.Pending || a.Status == Verdict.Running)
                .Count();

        public void GetMatrix(out Verdict[] usedVerdicts, out int[,] matrix)
        {
            var optts = Differences
                .Where(s => s.NewJudging.Status != Verdict.Pending)
                .Where(s => s.NewJudging.Status != Verdict.Running);

            var verds = optts
                .Select(s => new { Old = s.OldJudging.Status, New = s.NewJudging.Status })
                .ToList();

            usedVerdicts = verds
                .SelectTwo(a => a.New, a => a.Old)
                .Distinct()
                .OrderBy(v => v)
                .ToArray();

            matrix = new int[usedVerdicts.Length, usedVerdicts.Length];
            var rev = new int[13];

            for (int i = 0; i < usedVerdicts.Length; i++)
                rev[(int)usedVerdicts[i]] = i;

            foreach (var s in verds)
                matrix[rev[(int)s.Old], rev[(int)s.New]]++;
        }
    }
}
