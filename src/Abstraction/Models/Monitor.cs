using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Models
{
    public class Monitor : IEnumerable<IGrouping<Team, TeamMemberModel>>
    {
        private readonly ILookup<int, TeamMemberModel> _members;
        private readonly IReadOnlyDictionary<int, Team> _teams;

        public Monitor(
            List<Team> teams,
            List<TeamMemberModel> members)
        {
            _teams = teams.ToDictionary(t => t.TeamId);
            _members = members.ToLookup(t => t.TeamId);
        }

        public int Count => _teams.Count;

        public IEnumerator<IGrouping<Team, TeamMemberModel>> GetEnumerator()
        {
            foreach (var team in _teams.Values)
            {
                yield return new TeamInfo(team, _members[team.TeamId]);
            }
        }

        public IGrouping<Team, TeamMemberModel>? this[int teamId]
        {
            get
            {
                return _teams.TryGetValue(teamId, out var team)
                    ? new TeamInfo(team, _members[team.TeamId])
                    : null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class TeamInfo : IGrouping<Team, TeamMemberModel>
        {
            private readonly Team _key;
            private readonly IEnumerable<TeamMemberModel> _values;

            public Team Key => _key;

            public IEnumerator<TeamMemberModel> GetEnumerator() => _values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

            public TeamInfo(Team key, IEnumerable<TeamMemberModel> values)
            {
                _key = key;
                _values = values;
            }
        }
    }
}
