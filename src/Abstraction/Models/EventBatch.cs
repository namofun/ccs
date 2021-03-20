using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Models
{
    public sealed class EventBatch : IEnumerable<Ccs.Entities.Event>, IDisposable
    {
        private readonly List<Ccs.Entities.Event> _events;
        private readonly DateTimeOffset _preferredTime;
        private readonly int _cid;
        private ILoggable? _loggable;

        public int Count => _events.Count;

        public Task LogAsync(string message)
        {
            return _loggable?.LogAsync(message) ?? Task.CompletedTask;
        }

        public EventBatch(int cid, DateTimeOffset preferredTime, ILoggable loggable)
        {
            _preferredTime = preferredTime;
            _events = new List<Ccs.Entities.Event>();
            _cid = cid;
            _loggable = loggable;
        }

        public void AddCreate(IEnumerable<Specifications.AbstractEvent> events)
        {
            _events.AddRange(events.Select(e =>
            {
                e.FakeDefaultTime = _preferredTime;
                return e.ToEvent("create", _cid);
            }));
        }

        public void AddUpdate(IEnumerable<Specifications.AbstractEvent> events)
        {
            _events.AddRange(events.Select(e =>
            {
                e.FakeDefaultTime = _preferredTime;
                return e.ToEvent("update", _cid);
            }));
        }

        public void AddCreate<T>(IEnumerable<T> events, Func<T, Specifications.AbstractEvent> shaper)
        {
            _events.AddRange(events.Select(ee =>
            {
                var e = shaper(ee);
                e.FakeDefaultTime = _preferredTime;
                return e.ToEvent("create", _cid);
            }));
        }

        public void AddUpdate<T>(IEnumerable<T> events, Func<T, Specifications.AbstractEvent> shaper)
        {
            _events.AddRange(events.Select(ee =>
            {
                var e = shaper(ee);
                e.FakeDefaultTime = _preferredTime;
                return e.ToEvent("update", _cid);
            }));
        }

        public void AddCreate(Specifications.AbstractEvent e)
        {
            e.FakeDefaultTime = _preferredTime;
            _events.Add(e.ToEvent("create", _cid));
        }

        public void AddUpdate(Specifications.AbstractEvent e)
        {
            e.FakeDefaultTime = _preferredTime;
            _events.Add(e.ToEvent("update", _cid));
        }

        public void Dispose()
        {
            _loggable = null;
            _events.Clear();
        }

        public IEnumerable<Ccs.Entities.Event> GetEnumerable()
        {
            return _events.OrderBy(e => e.EventTime);
        }

        public IEnumerator<Ccs.Entities.Event> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }

        public interface ILoggable
        {
            Task LogAsync(string message);
        }
    }
}
