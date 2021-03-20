#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// Defines a contract that represents the result of an action method.
    /// </summary>
    public abstract class LongRunningOperationResult : IActionResult
    {
        private ActionContext? _actionContext;
        private readonly string _contentType;

        /// <summary>
        /// Initialize the long running action.
        /// </summary>
        /// <param name="contentType">The output content type.</param>
        protected LongRunningOperationResult(string contentType)
        {
            _contentType = contentType;
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// This method is called by MVC to process the result of an action method.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token denoting whether the request has stopped.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Writes a string of <paramref name="content"/> to the output content.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <returns>The task for writing the content and flushing body stream.</returns>
        protected async Task WriteAsync(string content)
        {
            if (_actionContext == null)
            {
                throw new ObjectDisposedException(nameof(_actionContext));
            }

            var bytes = Encoding.UTF8.GetBytes(content);
            await _actionContext.HttpContext.Response.Body.WriteAsync(bytes.AsMemory());
            await _actionContext.HttpContext.Response.Body.FlushAsync();
        }

        /// <summary>
        /// Gets service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="T"/>.</returns>
        protected T GetService<T>() where T : class
        {
            if (_actionContext == null)
            {
                throw new ObjectDisposedException(nameof(_actionContext));
            }

            return _actionContext.HttpContext.RequestServices.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets context of type <typeparamref name="T"/> from the <see cref="IContestFeature"/>.
        /// </summary>
        /// <typeparam name="T">The type of context object to get.</typeparam>
        /// <returns>A context object of type <typeparamref name="T"/>.</returns>
        protected T GetContext<T>() where T : Ccs.Services.IContestContext
        {
            if (_actionContext == null)
            {
                throw new ObjectDisposedException(nameof(_actionContext));
            }

            return (T)_actionContext.HttpContext.Features.Get<IContestFeature>().Context;
        }

        /// <inheritdoc />
        async Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            try
            {
                _actionContext = context;

                context.HttpContext.Response.StatusCode = 200;
                context.HttpContext.Response.ContentType = _contentType;
                await ExecuteAsync(context.HttpContext.RequestAborted);
            }
            finally
            {
                _actionContext = null;
            }
        }
    }
}
