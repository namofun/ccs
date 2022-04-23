using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Awards such as medals, first to solve, etc.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Awards">More detail</a>
    /// </summary>
    public class Award : AbstractEvent
    {
        /// <summary>
        /// Identifier of the award
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Award citation, e.g. "Gold medal winner"
        /// </summary>
        [JsonPropertyName("citation")]
        public string Citation { get; }

        /// <summary>
        /// JSON array of team ids receiving this award
        /// </summary>
        /// <remarks>No meaning must be implied or inferred from the order of IDs. The array may be empty.</remarks>
        [JsonPropertyName("team_ids")]
        public string[] TeamIds { get; }

        /// <inheritdoc />
        protected override string EndpointType => "awards";

        /// <summary>
        /// Construct an <see cref="Award"/>.
        /// </summary>
        /// <param name="id">The award ID.</param>
        /// <param name="citation">The citation.</param>
        /// <param name="team_ids">The team IDs.</param>
        public Award(string id, string citation, string[] team_ids)
        {
            Id = id;
            Citation = citation;
            TeamIds = team_ids;
        }
    }
}
