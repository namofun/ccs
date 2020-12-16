using Ccs.Entities;
using Ccs.Models;
using MediatR;
using System.Collections.Generic;

namespace Ccs.Events
{
    public class ScoreboardSortEvent : IRequest<IEnumerable<IScoreboardRow>>
    {
        public IEnumerable<IScoreboardRow> Source { get; }

        public Contest Contest { get; }

        public bool IsPublic { get; }

        public ScoreboardSortEvent(Contest contest, IEnumerable<IScoreboardRow> source, bool isPublic)
        {
            Source = source;
            Contest = contest;
            IsPublic = isPublic;
        }
    }
}
