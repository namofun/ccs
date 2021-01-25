using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    /// <summary>
    /// Provides the basic abstraction for registration.
    /// </summary>
    /// <typeparam name="TInputModel">The input model.</typeparam>
    /// <typeparam name="TOutputModel">The output model.</typeparam>
    public interface IRegisterProvider<TInputModel, TOutputModel>
        where TInputModel : class
        where TOutputModel : class
    {
        /// <summary>
        /// Gets whether this provider is for jury or contestant.
        /// </summary>
        /// <returns><c>true</c> for jury; otherwise, <c>false</c>.</returns>
        bool JuryOrContestant { get; }

        /// <summary>
        /// Creates an empty input model with its fields initialized.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <returns>The task for creating an input model.</returns>
        Task<TInputModel> CreateInputModelAsync(RegisterProviderContext context);

        /// <summary>
        /// Validates the input content.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="model">The input model to validate.</param>
        /// <param name="modelState">The model state dictionary.</param>
        /// <returns>The task for validating, returning the validate result.</returns>
        Task ValidateAsync(RegisterProviderContext context, TInputModel model, ModelStateDictionary modelState);

        /// <summary>
        /// Executes the input model.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="model">The input model to execute.</param>
        /// <returns>The task for executing.</returns>
        Task<TOutputModel> ExecuteAsync(RegisterProviderContext context, TInputModel model);

        /// <summary>
        /// Renders the input view.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="output">The view container to render to.</param>
        /// <returns>The task for rendering.</returns>
        Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<TInputModel> output);

        /// <summary>
        /// Renders the output view.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="output">The view container to render to.</param>
        /// <returns>The task for rendering.</returns>
        Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<TOutputModel> output);
    }
}
