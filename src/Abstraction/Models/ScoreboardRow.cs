using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// The team name
        /// </summary>
        string TeamName { get; }

        /// <summary>
        /// The category ID
        /// </summary>
        int CategoryId { get; }

        /// <summary>
        /// The affiliation ID
        /// </summary>
        int AffiliationId { get; }

        /// <summary>
        /// The navigation to rank cache
        /// </summary>
        RankCache RankCache { get; }

        /// <summary>
        /// The navigation to score cache
        /// </summary>
        IEnumerable<ScoreCache> ScoreCache { get; }
    }

    /// <summary>
    /// The model class representing single row in the scoreboard.
    /// </summary>
    public class ScoreboardRow : IScoreboardRow
    {
        private RankCache? _rankCache;
        private IEnumerable<ScoreCache>? _scoreCache;

        /// <inheritdoc />
        public int TeamId { get; }

        /// <inheritdoc />
        public string TeamName { get; }

        /// <inheritdoc />
        public int CategoryId { get; }

        /// <inheritdoc />
        public int AffiliationId { get; }

        /// <inheritdoc />
        public RankCache RankCache => _rankCache ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public IEnumerable<ScoreCache> ScoreCache => _scoreCache ?? throw new InvalidOperationException();

        public ScoreboardRow(int teamId, string teamName, int catId, int affId)
        {
            TeamId = teamId;
            TeamName = teamName;
            CategoryId = catId;
            AffiliationId = affId;
        }

        public IScoreboardRow With(RankCache? rankCache, IEnumerable<ScoreCache>? scoreCache)
        {
            _rankCache = rankCache ?? RankCache.Empty;
            _scoreCache = scoreCache ?? Array.Empty<ScoreCache>();
            return this;
        }
    }
}
