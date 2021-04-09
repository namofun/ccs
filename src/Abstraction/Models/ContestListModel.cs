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
        public bool IsRegistered { get; }

        /// <summary>Whether user is jury of this contest</summary>
        public bool IsJury { get; }

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
        public ContestListModel(int id, string name, string shortName, DateTimeOffset? start, double? duration, int kind, int ranker, bool isPublic, int teamCount, int problemCount)
            : this(id, name, shortName, start,
                  duration.HasValue ? TimeSpan.FromSeconds(duration.Value) : default(TimeSpan?),
                  kind, ranker, isPublic, teamCount, problemCount, false, false)
        {
        }

        private ContestListModel(int id, string name, string shortName, DateTimeOffset? start, TimeSpan? duration, int kind, int ranker, bool isPublic, int teamCount, int problemCount, bool registered, bool isJury)
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
            IsRegistered = registered;
            IsJury = isJury;

            if (!start.HasValue)
                _state = 1; // Not Scheduled
            else if (!duration.HasValue || (start + duration) >= DateTimeOffset.Now)
                _state = 2; // Running or Waiting
            else
                _state = 3; // Ended
        }

        /// <summary>
        /// Make a copy of this model with the specified property.
        /// </summary>
        /// <param name="registered">Whether user has registered.</param>
        /// <param name="isJury">Whether user is jury of contest.</param>
        /// <returns>A new instance of model.</returns>
        public ContestListModel With(bool registered, bool isJury)
        {
            return new ContestListModel(
                ContestId,
                Name,
                ShortName,
                StartTime,
                Duration,
                Kind,
                RankingStrategy,
                IsPublic,
                TeamCount,
                ProblemCount,
                registered,
                isJury);
        }

        /// <inheritdoc />
        public int CompareTo(ContestListModel other)
        {
            if (Kind != other.Kind)
            {
                return Kind.CompareTo(other.Kind);
            }
            else if (Kind == 2)
            {
                // When they are all problemsets
                // The earlier created stays last.
                return other.ContestId.CompareTo(ContestId);
            }
            else if (Kind == 1)
            {
                // When they are all gyms
                if (!StartTime.HasValue && !other.StartTime.HasValue)
                {
                    // If this and that both have no start time, compare with cid.
                    return ContestId.CompareTo(other.ContestId);
                }
                else if (StartTime.HasValue && other.StartTime.HasValue)
                {
                    // If both have start time, then the early one is bigger.
                    return other.StartTime.Value.CompareTo(StartTime.Value);
                }
                else
                {
                    // If this has a start time and that doesn't, this > that.
                    return StartTime.HasValue ? 1 : -1;
                }
            }
            else
            {
                // When they are all contests
                if (_state != other._state)
                {
                    // When they are not the same state, compare with current.
                    return _state.CompareTo(other._state);
                }
                else if (_state == 1)
                {
                    // When they are both not scheduled, compare with cid.
                    return ContestId.CompareTo(other.ContestId);
                }
                else if (_state == 2)
                {
                    // When they are both running, the earlier first.
                    return StartTime!.Value.CompareTo(other.StartTime!.Value);
                }
                else
                {
                    // When they are both stopped, the earlier last.
                    return other.StartTime!.Value.CompareTo(StartTime!.Value);
                }
            }
        }
    }
}
