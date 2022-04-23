using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// The abstract event class for serializing CCS events.
    /// </summary>
    public abstract class AbstractEvent
    {
        public static DateTimeOffset TrimToMilliseconds(DateTimeOffset value)
            => DateTimeOffset.FromUnixTimeMilliseconds(value.ToUnixTimeMilliseconds()).ToOffset(value.Offset);

        private class CdsCompatibleDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
                => throw new InvalidOperationException();

            public override void Write(
                Utf8JsonWriter writer,
                DateTimeOffset value,
                JsonSerializerOptions options)
                => writer.WriteStringValue(TrimToMilliseconds(value));
        }

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
                Converters = { new TimeSpanJsonConverter(), new CdsCompatibleDateTimeOffsetJsonConverter() },
            };

        /// <summary>
        /// Abstract ID
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Convert the event to <see cref="Entities.Event"/>.
        /// </summary>
        /// <param name="action">The event action.</param>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The converted event.</returns>
        public Entities.Event ToEvent(string action, int cid)
        {
            return new Entities.Event
            {
                Action = action,
                Content = action == "delete"
                    ? $"{{\"id\":\"{Id}\"}}"
                    : JsonSerializer.Serialize(this, GetType(), _jsonOptions),
                ContestId = cid,
                EndpointId = Id,
                EndpointType = EndpointType,
                EventTime = GetTime(action),
            };
        }

        /// <summary>
        /// Gets the endpoint type.
        /// </summary>
        [JsonIgnore]
        protected abstract string EndpointType { get; }

        /// <summary>
        /// Gets or sets the fake default time.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? FakeDefaultTime { get; set; }

        /// <summary>
        /// Get the event happening time.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The happening datetime.</returns>
        protected virtual DateTimeOffset GetTime(string action) => FakeDefaultTime ?? DateTimeOffset.Now;
    }
}
