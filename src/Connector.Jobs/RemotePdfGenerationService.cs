using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xylab.Contesting.Connector.Jobs
{
    public class RemotePdfGenerationService
    {
        private readonly HttpClient _client;
        private readonly ExportPdfOptions _options;

        public RemotePdfGenerationService(
            HttpClient client,
            IOptions<ExportPdfOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task<byte[]> GenerateAsync(string html)
        {
            if (_options.Url == null)
            {
                throw new InvalidOperationException("https://github.com/alvarcarto/url-to-pdf-api not defined.");
            }

            _client.Timeout = TimeSpan.FromMinutes(3);
            using var message = await _client.PostAsync(
                _options.Url + "?emulateScreenMedia=false&pdf.margin.top=1cm&pdf.margin.right=1cm&pdf.margin.bottom=1cm&pdf.margin.left=1cm",
                new StringContent(html, System.Text.Encoding.UTF8, "text/html"));

            message.EnsureSuccessStatusCode();
            return await message.Content.ReadAsByteArrayAsync();
        }
    }
}
