using System;
using System.Collections.Generic;
using Xylab.Tenant.Entities;

namespace Xylab.Contesting.Models
{
    /// <summary>
    /// The model class for team information in analysis.
    /// </summary>
    public class AnalyticalTeam
    {
        private Affiliation? _affiliation;
        private Category? _category;

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; }

        /// <summary>
        /// The team name
        /// </summary>
        public string TeamName { get; }

        /// <summary>
        /// The affiliation ID
        /// </summary>
        public int AffiliationId { get; }

        /// <summary>
        /// The category ID
        /// </summary>
        public int CategoryId { get; }

        /// <summary>
        /// The affiliation
        /// </summary>
        public Affiliation Affiliation => _affiliation ?? throw new InvalidOperationException();

        /// <summary>
        /// The category
        /// </summary>
        public Category Category => _category ?? throw new InvalidOperationException();

        /// <summary>
        /// Instantiate an <see cref="AnalyticalTeam"/>.
        /// </summary>
        public AnalyticalTeam(int teamId, string teamName, int affId, int catId)
        {
            TeamId = teamId;
            TeamName = teamName;
            AffiliationId = affId;
            CategoryId = catId;
        }

        /// <summary>
        /// Align with joined tables.
        /// </summary>
        public AnalyticalTeam With(
            IReadOnlyDictionary<int, Category> categories,
            IReadOnlyDictionary<int, Affiliation> affiliations)
        {
            _affiliation = affiliations[AffiliationId];
            _category = categories[CategoryId];
            return this;
        }
    }
}
