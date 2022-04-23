using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Languages that are available for submission at the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Languages">More detail</a>
    /// </summary>
    public class Language : AbstractEvent
    {
        /// <summary>
        /// Identifier of the language from table below
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Name of the language
        /// </summary>
        /// <remarks>Might not match table below, e.g. if localised.</remarks>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// File extensions of the language
        /// </summary>
        [JsonPropertyName("extensions")]
        public string[] Extensions { get; }

        /// <summary>
        /// If allow judge solutions in this language
        /// </summary>
        [JsonPropertyName("allow_judge")]
        public bool AllowJudge { get; }

        /// <summary>
        /// If allow submit solutions in this language
        /// </summary>
        [JsonPropertyName("allow_subit")]
        public bool AllowSubmit { get; }

        /// <summary>
        /// Time factor for running solutions
        /// </summary>
        [JsonPropertyName("time_factor")]
        public double TimeFactor { get; }

        /// <inheritdoc />
        protected override string EndpointType => "languages";

        /// <summary>
        /// Construct a <see cref="Language"/>.
        /// </summary>
        /// <param name="l">The language entity.</param>
        public Language(Xylab.Polygon.Entities.Language l)
        {
            AllowJudge = l.AllowJudge;
            AllowSubmit = l.AllowSubmit;
            TimeFactor = l.TimeFactor;
            Extensions = new[] { l.FileExtension };
            Id = l.Id;
            Name = l.Name;
        }
    }
}
