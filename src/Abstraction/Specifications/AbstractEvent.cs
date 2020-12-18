﻿using System;
using System.Text.Json.Serialization;

namespace Ccs.Specifications
{
    /// <summary>
    /// The abstract event class for serializing CCS events.
    /// </summary>
    public abstract class AbstractEvent
    {
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
                    : this.ToJson(),
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
        /// Get the event happening time.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The happening datetime.</returns>
        protected virtual DateTimeOffset GetTime(string action) => DateTimeOffset.Now;
    }
}