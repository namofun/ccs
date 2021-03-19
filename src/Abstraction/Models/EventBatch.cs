using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ccs.Models
{
    public sealed class EventBatch : IEnumerable<Ccs.Entities.Event>, IDisposable
    {
        private readonly List<Ccs.Entities.Event> _events;
        private readonly DateTimeOffset _preferredTime;
        private readonly int _cid;

        public int Count => _events.Count;

        public EventBatch(int cid, DateTimeOffset preferredTime)
        {
            _preferredTime = preferredTime;
            _events = new List<Ccs.Entities.Event>();
            _cid = cid;
        }

        public void AddCreate(IEnumerable<Specifications.AbstractEvent> events)
        {
            _events.AddRange(events.Select(e =>
            {
                e.FakeDefaultTime = _preferredTime;
                return e.ToEvent("create", _cid);
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

        public void AddCreate(Specifications.AbstractEvent e)
        {
            e.FakeDefaultTime = _preferredTime;
            _events.Add(e.ToEvent("create", _cid));
        }

        public void Dispose()
        {
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
    }
}
