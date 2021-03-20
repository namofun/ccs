using Ccs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// The action result for producing <c>application/x-ndjson</c> response via contest store.
    /// </summary>
    public class EventFeedResult : LongRunningOperationResult
    {
        private readonly IJuryContext _ctx;
        private readonly string[] _type;
        private readonly bool _keepAlive;
        private int after, step;

        public EventFeedResult(
            IJuryContext context,
            string[] type,
            bool stream,
            int sinceid)
            : base("application/x-ndjson")
        {
            _ctx = context;
            _type = type;
            _keepAlive = stream;
            after = sinceid;
            step = 0;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var events = await _ctx.ListEventsAsync(_type, after);
                if (events.Count > 0) after = events[^1].Id;

                foreach (var item in events)
                {
                    await WriteAsync($"{{\"type\":\"{item.EndpointType}\",\"id\":\"{item.EndpointId}\",\"op\":\"{item.Action}\",\"data\":{item.Content}}}\n");
                }

                if (!_keepAlive) break;
                step++;

                if (step >= 30) // 30s no content and flush
                {
                    await WriteAsync("\n");
                    step = 0;
                }

                if (events.Count == 0) await Task.Delay(1000);
            }
        }
    }
}
