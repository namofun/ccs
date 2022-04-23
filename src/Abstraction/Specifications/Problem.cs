using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// The problems to be solved in the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Problems">More detail</a>
    /// </summary>
    public class Problem : AbstractEvent
    {
        /// <summary>
        /// Identifier of the problem, at the WFs the directory name of the problem archive
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Label of the problem on the scoreboard, typically a single capitalized letter
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; }

        /// <summary>
        /// Name of the problem
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Ordering of problems on the scoreboard
        /// </summary>
        [JsonPropertyName("ordinal")]
        public int Ordinal { get; }

        /// <summary>
        /// Internal identifier
        /// </summary>
        [JsonPropertyName("internalid")]
        public int InternalId { get; }

        /// <summary>
        /// Time limit in seconds per test data set (i.e. per single run)
        /// </summary>
        [JsonPropertyName("time_limit")]
        public double TimeLimit { get; }

        /// <summary>
        /// Hexadecimal RGB value of problem color as specified in HTML hexadecimal colors, e.g. '#AC00FF' or '#fff'
        /// </summary>
        [JsonPropertyName("rgb")]
        public string Rgb { get; }

        ///// <summary>
        ///// Human readable color description associated to the RGB value
        ///// </summary>
        //[JsonPropertyName("color")]
        //public string Color { get; }

        /// <summary>
        /// Number of test data sets
        /// </summary>
        [JsonPropertyName("test_data_count")]
        public int TestDataCount { get; }

        /// <inheritdoc />
        protected override string EndpointType => "problems";

        /// <summary>
        /// Construct a <see cref="Problem"/>.
        /// </summary>
        /// <param name="cp">The problem model.</param>
        public Problem(Models.ProblemModel cp)
        {
            Ordinal = cp.Rank;
            Label = cp.ShortName;
            InternalId = cp.ProblemId;
            Id = $"{cp.ProblemId}";
            TimeLimit = cp.TimeLimit / 1000.0;
            Name = cp.Title;
            Rgb = cp.Color;
            TestDataCount = cp.TestcaseCount;
        }
    }
}
