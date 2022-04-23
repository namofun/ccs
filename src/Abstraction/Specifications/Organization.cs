using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Teams can be associated with organizations which will have some associated information, e.g. a logo. Typically organizations will be universities.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Organizations">More detail</a>
    /// </summary>
    public class Organization : AbstractEvent
    {
        /// <summary>
        /// Identifier of the organization
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// External identifier from ICPC CMS
        /// </summary>
        [JsonPropertyName("icpc_id")]
        public string IcpcId { get; }

        /// <summary>
        /// Short display name of the organization
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Short display name of the organization
        /// </summary>
        [JsonPropertyName("shortname")]
        public string ShortName { get; }

        /// <summary>
        /// Full organization name if too long for normal display purposes.
        /// </summary>
        [JsonPropertyName("formal_name")]
        public string FormalName { get; }

        /// <summary>
        /// ISO 3-letter code of the organization's country
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; }

        /// <inheritdoc />
        protected override string EndpointType => "organizations";

        /// <summary>
        /// Construct an <see cref="Organization"/>.
        /// </summary>
        /// <param name="a">The affiliation entity.</param>
        public Organization(Xylab.Tenant.Entities.Affiliation a)
        {
            Id = a.Abbreviation;
            Name = ShortName = a.Abbreviation.ToUpper();
            Country = a.CountryCode;
            FormalName = a.Name;
            IcpcId = $"{a.Id}";
        }
    }
}
