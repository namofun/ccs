using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccs.Models
{
    public class ContestListModel : IComparable<ContestListModel>
    {
        private readonly int _state;

        /// <inheritdoc cref="Contest.Id" />
        public int ContestId { get; }

        /// <inheritdoc cref="Contest.Name" />
        public string Name { get; }

        /// <inheritdoc cref="Contest.ShortName" />
        public string ShortName { get; }

        /// <inheritdoc cref="Contest.StartTime" />
        public DateTimeOffset? StartTime { get; }

        /// <summary>The contest duration</summary>
        public TimeSpan? Duration { get; }

        /// <inheritdoc cref="Contest.Kind" />
        public int Kind { get; }

        /// <inheritdoc cref="Contest.RankingStrategy" />
        public int RankingStrategy { get; }

        /// <inheritdoc cref="Contest.IsPublic" />
        public bool IsPublic { get; }

        /// <inheritdoc cref="Contest.TeamCount" />
        public int TeamCount { get; }

        /// <inheritdoc cref="Contest.ProblemCount" />
        public int ProblemCount { get; }

        /// <summary>
        /// Construct a <see cref="ContestListModel"/>.
        /// </summary>
        /// <param name="id">The contest ID.</param>
        /// <param name="name">The contest name.</param>
        /// <param name="shortName">The contest short name.</param>
        /// <param name="start">The start time.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="kind">The contest kind.</param>
        /// <param name="ranker">The ranking strategy.</param>
        /// <param name="isPublic">Whether to show to public.</param>
        /// <param name="problemCount">The count of problems.</param>
        /// <param name="teamCount">The count of teams.</param>
        public ContestListModel(int id, string name, string shortName, DateTimeOffset? start, TimeSpan? duration, int kind, int ranker, bool isPublic, int teamCount, int problemCount)
        {
            ContestId = id;
            Name = name;
            ShortName = shortName;
            StartTime = start;
            Duration = duration;
            Kind = kind;
            RankingStrategy = ranker;
            IsPublic = isPublic;
            TeamCount = teamCount;
            ProblemCount = problemCount;

            if (!StartTime.HasValue)
                _state = 1; // Not Scheduled
            else if (!Duration.HasValue || (StartTime + Duration) >= DateTimeOffset.Now)
                _state = 2; // Running or Waiting
            else
                _state = 3; // Ended
        }


        [Obsolete]
        public bool IsRegistered { get; set; }

        [Obsolete]
        public bool OpenRegister { get; set; }



        public int CompareTo(ContestListModel other)
        {
            if (Kind != other.Kind)
            {
                return Kind.CompareTo(other.Kind);
            }
            else if (Kind == 1)
            {
                if (!StartTime.HasValue && !other.StartTime.HasValue)
                    return ContestId.CompareTo(other.ContestId);
                else if (StartTime.HasValue && other.StartTime.HasValue)
                    return StartTime.Value.CompareTo(other.StartTime.Value);
                else
                    return StartTime.HasValue ? 1 : -1;
            }
            else
            {
                int t1 = _state, t2 = other._state;
                if (t1 != t2) return t1.CompareTo(t2);
                if (t1 == 1) return ContestId.CompareTo(other.ContestId);
                if (t1 == 2) return StartTime!.Value.CompareTo(other.StartTime!.Value);
                return other.StartTime!.Value.CompareTo(StartTime!.Value);
            }
        }
    }
}
