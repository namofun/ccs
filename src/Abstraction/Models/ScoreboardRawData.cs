using Ccs.Entities;
using System.Collections.Generic;

namespace Ccs.Models
{
    /// <summary>
    /// The scoreboard raw data.
    /// </summary>
    public class ScoreboardRawData
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; }

        /// <summary>
        /// The rank entities
        /// </summary>
        public IEnumerable<RankCache> RankCache { get; }

        /// <summary>
        /// The score entities
        /// </summary>
        public IEnumerable<ScoreCache> ScoreCache { get; }

        /// <summary>
        /// Initialize the raw data.
        /// </summary>
        public ScoreboardRawData(int cid, IEnumerable<RankCache> ranks, IEnumerable<ScoreCache> scores)
        {
            ContestId = cid;
            RankCache = ranks;
            ScoreCache = scores;
        }
    }
}
