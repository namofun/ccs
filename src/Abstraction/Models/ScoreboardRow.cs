using Ccs.Entities;
using System.Collections.Generic;

namespace Ccs.Models
{
    /// <summary>
    /// The model interface representing single row in the scoreboard.
    /// </summary>
    public interface IScoreboardRow
    {
        /// <summary>
        /// The team ID
        /// </summary>
        int TeamId { get; }

        /// <summary>
        /// The navigation to rank cache
        /// </summary>
        RankCache? RankCache { get; }

        /// <summary>
        /// The navigation to score cache
        /// </summary>
        ICollection<ScoreCache> ScoreCache { get; }
    }
}
