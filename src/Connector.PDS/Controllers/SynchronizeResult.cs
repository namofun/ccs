using Ccs.Connector.PlagiarismDetect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Models;
using Plag.Backend.Services;
using SatelliteSite.ContestModule;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ccs.Connector.PlagiarismDetect.Controllers
{
    public class SynchronizeResult : IActionResult
    {
        public SynchronizationOptionsModel Model { get; }

        public PlagiarismSet PlagiarismSet { get; }

        public SynchronizeResult(SynchronizationOptionsModel model, PlagiarismSet plagiarismSet)
        {
            Model = model;
            PlagiarismSet = plagiarismSet;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var pds = context.HttpContext.RequestServices.GetRequiredService<IPlagiarismDetectService>();
            var ccs = context.HttpContext.Features.Get<IContestFeature>().Context;

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await WriteAsync("<p>Hahahahahhahaha</p>");
            }

            async Task WriteAsync(string content)
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                await context.HttpContext.Response.Body.WriteAsync(bytes.AsMemory());
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }
}
