using Ccs.Entities;
using MediatR;
using System.Collections.Generic;

namespace Ccs.Events
{
    public class ScoreboardSortEvent : IRequest<IEnumerable<Team>>
    {
        public IEnumerable<Team> Source { get; }

        public Contest Contest { get; }

        public bool IsPublic { get; }

        public ScoreboardSortEvent(Contest contest, IEnumerable<Team> source, bool isPublic)
        {
            Source = source;
            Contest = contest;
            IsPublic = isPublic;
        }
    }
}
