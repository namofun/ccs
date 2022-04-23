using System;

namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for contest event used in CDS/CCS API.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The event ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The event time
        /// </summary>
        public DateTimeOffset EventTime { get; set; }

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The endpoint entity type
        /// </summary>
        public string EndpointType { get; set; }

        /// <summary>
        /// The endpoint entity ID
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// The event action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The event content
        /// </summary>
        public string Content { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="Event"/>.
        /// </summary>
        public Event()
        {
        }
#pragma warning restore CS8618
    }
}
