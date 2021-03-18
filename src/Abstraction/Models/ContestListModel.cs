using Ccs.Entities;
using System;

namespace Ccs.Models
{
    public class ContestListModel : IComparable<ContestListModel>
    {
        private readonly int _state;

        /// <inheritdoc cref="IContestInformation.Id" />
        public int ContestId { get; }

        /// <inheritdoc cref="IContestInformation.Name" />
        public string Name { get; }

        /// <inheritdoc cref="IContestInformation.ShortName" />
        public string ShortName { get; }

        /// <inheritdoc cref="IContestTime.StartTime" />
        public DateTimeOffset? StartTime { get; }

        /// <summary>The contest duration</summary>
        public TimeSpan? Duration { get; }

        /// <inheritdoc cref="IContestInformation.Kind" />
        public int Kind { get; }

        /// <inheritdoc cref="IContestInformation.RankingStrategy" />
        public int RankingStrategy { get; }

        /// <inheritdoc cref="IContestInformation.IsPublic" />
        public bool IsPublic { get; }

        /// <inheritdoc cref="Contest.TeamCount" />
        public int TeamCount { get; }

        /// <inheritdoc cref="Contest.ProblemCount" />
        public int ProblemCount { get; }

        /// <summary>Whether user has registered this contest</summary>
        public bool IsRegistered { get; set; }

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
        /// <param name="registered">Whether user has registered.</param>
        public ContestListModel(int id, string name, string shortName, DateTimeOffset? start, double? duration, int kind, int ranker, bool isPublic, int teamCount, int problemCount, bool registered)
        {
            ContestId = id;
            Name = name;
            ShortName = shortName;
            StartTime = start;
            Duration = duration.HasValue ? TimeSpan.FromSeconds(duration.Value) : default(TimeSpan?);
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

        /// <inheritdoc cref="ContestListModel(int, string, string, DateTimeOffset?, double?, int, int, bool, int, int, bool)" />
        public ContestListModel(int id, string name, string shortName, DateTimeOffset? start, double? duration, int kind, int ranker, bool isPublic, int teamCount, int problemCount)
            : this(id, name, shortName, start, duration, kind, ranker, isPublic, teamCount, problemCount, false)
        {
        }

        [Obsolete]
        public bool OpenRegister { get; set; }

        /// <inheritdoc />
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
