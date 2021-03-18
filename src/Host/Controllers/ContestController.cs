using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatelliteSite.Controllers
{
    public class ContestController : ViewControllerBase
    {
        private readonly IContestRepository2 _repository;

        public ContestController(IContestRepository2 repository)
        {
            _repository = repository;
        }

        [HttpGet("/get")]
        public async Task<IActionResult> Get([FromServices] JavaScriptEncoder encoder)
        {
            var res = await _repository.ListAsync(User);

            return Json(
                new { res.Count, res },
                new System.Text.Json.JsonSerializerOptions
                {
                    Converters = { new TimeSpanJsonConverter() },
                    Encoder = encoder,
                    WriteIndented = true,
                });
        }
    }
}
