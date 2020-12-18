using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// The action result for producing <c>application/x-ndjson</c> response via contest store.
    /// </summary>
    public class EventFeedResult : IActionResult
    {
        private readonly IContestStore _store;
        private readonly int _cid;
        private readonly string[] _type;
        private readonly bool _keepAlive;
        private int after, step;

        /// <summary>
        /// Instantiate an <see cref="EventFeedResult"/>.
        /// </summary>
        /// <param name="store">The storage interface to fetch events.</param>
        /// <param name="cid">The contest ID.</param>
        /// <param name="type">Allowed endpoint types.</param>
        /// <param name="stream">Whether to stream output.</param>
        /// <param name="sinceid">The first event ID.</param>
        public EventFeedResult(IContestStore store, int cid, string[] type, bool stream, int sinceid)
        {
            _store = store;
            _cid = cid;
            _type = type;
            _keepAlive = stream;
            after = sinceid;
            step = 0;
        }

        /// <inheritdoc />
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "application/x-ndjson";

            while (true)
            {
                if (context.HttpContext.RequestAborted.IsCancellationRequested) break;

                var events = await _store.FetchEventAsync(_cid, _type, after);
                if (events.Count > 0) after = events[^1].Id;
                var newline = Encoding.UTF8.GetBytes("\n");

                foreach (var item in events)
                {
                    var beforeT = $"{{\"type\":\"{item.EndpointType}\",\"id\":\"{item.EndpointId}\",\"op\":\"{item.Action}\",\"data\":{item.Content}}}\n";
                    var before = Encoding.UTF8.GetBytes(beforeT);
                    await response.Body.WriteAsync(before, 0, before.Length);
                    await response.Body.FlushAsync();
                }

                if (!_keepAlive) break;
                step++;

                if (step >= 30) // 30s no content and flush
                {
                    await response.Body.WriteAsync(newline, 0, newline.Length);
                    await response.Body.FlushAsync();
                    step = 0;
                }

                if (events.Count == 0) await Task.Delay(1000);
            }
        }
    }
}
