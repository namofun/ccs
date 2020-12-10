using System.Text.Json.Serialization;

namespace Ccs.Specifications
{
    /// <summary>
    /// Grouping of teams. At the World Finals these are the super regions, at regionals these are often different sites.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Groups">More detail</a>
    /// </summary>
    public class Group : AbstractEvent
    {
        /// <summary>
        /// Identifier of the group
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// External identifier from ICPC CMS
        /// </summary>
        [JsonPropertyName("icpc_id")]
        public string? IcpcId { get; }

        /// <summary>
        /// If group should be hidden from scoreboard
        /// </summary>
        [JsonPropertyName("hidden")]
        public bool Hidden { get; }

        /// <summary>
        /// Name of the group
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Type of this group via scoreboard
        /// </summary>
        [JsonPropertyName("sortorder")]
        public int SortOrder { get; }

        /// <summary>
        /// Type of this group via color
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; }

        /// <inheritdoc />
        protected override string EndpointType => "groups";

        /// <summary>
        /// Construct a <see cref="Group"/>.
        /// </summary>
        /// <param name="c">The category entity.</param>
        public Group(Tenant.Entities.Category c)
        {
            Hidden = !c.IsPublic;
            Color = c.Color;
            Id = $"{c.Id}";
            Name = c.Name;
            SortOrder = c.SortOrder;
        }
    }
}
