using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Teams competing in the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Teams">More detail</a>
    /// </summary>
    public class Team : AbstractEvent
    {
        /// <summary>
        /// Identifier of the team
        /// </summary>
        /// <remarks>Usable as a label, at WFs normally the team seat number.</remarks>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// External identifier from ICPC CMS
        /// </summary>
        [JsonPropertyName("icpc_id")]
        public string? IcpcId { get; }

        /// <summary>
        /// Name of the team
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Identifier of the organization (e.g. university or other entity) that this team is affiliated to
        /// </summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; }

        /// <summary>
        /// Identifiers of the group(s) this team is part of (at ICPC WFs these are the super-regions)
        /// </summary>
        /// <remarks>No meaning must be implied or inferred from the order of IDs. The array may be empty.</remarks>
        [JsonPropertyName("group_ids")]
        public string[] GroupIds { get; }

        /// <summary>
        /// Team members
        /// </summary>
        [JsonPropertyName("members")]
        public string[]? Members { get; }

        /// <inheritdoc />
        protected override string EndpointType => "teams";

        /// <summary>
        /// Construct a <see cref="Team"/>.
        /// </summary>
        /// <param name="t">The team entity.</param>
        /// <param name="a">The affiliation entity.</param>
        public Team(Entities.Team t, Xylab.Tenant.Entities.Affiliation a)
        {
            GroupIds = new[] { $"{t.CategoryId}" };
            OrganizationId = a.Abbreviation;
            Id = $"{t.TeamId}";
            Name = t.TeamName;
        }
    }
}
