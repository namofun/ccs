using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Extension methods for ccs related things.
    /// </summary>
    public static class ServicingExtensions
    {
        /// <summary>
        /// Converts the store as a <see cref="IContestQueryableStore"/>.
        /// </summary>
        /// <param name="store">The <see cref="IContestRepository"/>.</param>
        /// <returns>The <see cref="IContestQueryableStore"/>.</returns>
        public static IContestQueryableStore GetQueryableStore(this IContestRepository store)
            => store as IContestQueryableStore
                ?? throw new InvalidOperationException("This store is not a IQueryable store.");

        /// <summary>
        /// Converts the store as a <see cref="IContestQueryableStore"/>.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <returns>The <see cref="IContestQueryableStore"/>.</returns>
        public static IContestQueryableStore GetQueryableStore(this IContestContext store)
            => store as IContestQueryableStore
                ?? throw new InvalidOperationException("This store is not a IQueryable store.");

        /// <summary>
        /// Emits a create event.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <param name="event">The event entity.</param>
        /// <returns>The task for emitting an event.</returns>
        public static Task EmitCreateEventAsync(this IContestContext store, Specifications.AbstractEvent @event)
        {
            return store.EmitEventAsync(@event, "create");
        }

        /// <summary>
        /// Emits an update event.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <param name="event">The event entity.</param>
        /// <returns>The task for emitting an event.</returns>
        public static Task EmitUpdateEventAsync(this IContestContext store, Specifications.AbstractEvent @event)
        {
            return store.EmitEventAsync(@event, "update");
        }

        /// <summary>
        /// Emits a create event.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <param name="events">The event entities.</param>
        /// <returns>The task for emitting an event.</returns>
        public static async Task EmitCreateEventAsync(this IContestContext store, IEnumerable<Specifications.AbstractEvent> events)
        {
            foreach (var @event in events) await store.EmitEventAsync(@event, "create");
        }

        /// <summary>
        /// Emits a create event.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <param name="events">The event source.</param>
        /// <param name="shaper">The event entity shaper.</param>
        /// <returns>The task for emitting an event.</returns>
        public static async Task EmitCreateEventAsync<T>(this IContestContext store, IEnumerable<T> events, Func<T, Specifications.AbstractEvent> shaper)
        {
            foreach (var @event in events) await store.EmitEventAsync(shaper(@event), "create");
        }

        /// <summary>
        /// Emits a create event.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <param name="events">The event source.</param>
        /// <param name="shaper">The event entity shaper.</param>
        /// <returns>The task for emitting an event.</returns>
        public static async Task EmitCreateEventAsync<T>(this IContestContext store, IReadOnlyList<T> events, Func<T, int, Specifications.AbstractEvent> shaper)
        {
            for (int i = 0; i < events.Count; i++) await store.EmitEventAsync(shaper(events[i], i), "create");
        }
    }
}
