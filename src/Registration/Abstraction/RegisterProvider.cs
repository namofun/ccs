using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace Ccs.Registration
{
    /// <summary>
    /// Provides the basic abstraction for registration.
    /// </summary>
    public interface IRegisterProvider
    {
        /// <summary>
        /// The name of registration provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The icon of registration provider.
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Gets whether this provider is for jury or contestant.
        /// </summary>
        /// <returns><c>true</c> for jury; otherwise, <c>false</c>.</returns>
        bool JuryOrContestant { get; }

        /// <summary>
        /// The display order of registration provider.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Creates an empty input model with its fields initialized.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <returns>The task for creating an input model.</returns>
        Task<object> CreateInputModelAsync(RegisterProviderContext context);

        /// <summary>
        /// Reads the existing model from controller.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="controller">The running controller.</param>
        /// <returns>The task for reading form values.</returns>
        Task<bool> ReadAsync(object model, ControllerBase controller);

        /// <summary>
        /// Validates the input content.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="model">The input model to validate.</param>
        /// <param name="modelState">The model state dictionary.</param>
        /// <returns>The task for validating, returning the validate result.</returns>
        Task ValidateAsync(RegisterProviderContext context, object model, ModelStateDictionary modelState);

        /// <summary>
        /// Executes the input model.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="model">The input model to execute.</param>
        /// <returns>The task for executing, returning the output model.</returns>
        Task<object> ExecuteAsync(RegisterProviderContext context, object model);

        /// <summary>
        /// Renders the input view.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="output">The view container to render to.</param>
        /// <returns>The task for rendering.</returns>
        Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput output);

        /// <summary>
        /// Renders the output view.
        /// </summary>
        /// <param name="context">The register provider context.</param>
        /// <param name="output">The view container to render to.</param>
        /// <returns>The task for rendering.</returns>
        Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput output);

        /// <summary>
        /// Creates the <see cref="RegisterProviderOutput"/> for input rendering.
        /// </summary>
        /// <returns>The created <see cref="RegisterProviderOutput"/>.</returns>
        RegisterProviderOutput CreateInputRenderer(
            ViewContext viewContext,
            object model,
            IModelExpressionProvider modelExpressionProvider,
            IJsonHelper jsonHelper,
            IViewComponentHelper viewComponentHelper,
            IUrlHelper urlHelper);

        /// <summary>
        /// Creates the <see cref="RegisterProviderOutput"/> for output rendering.
        /// </summary>
        /// <returns>The created <see cref="RegisterProviderOutput"/>.</returns>
        RegisterProviderOutput CreateOutputRenderer(
            ViewContext viewContext,
            object model,
            IModelExpressionProvider modelExpressionProvider,
            IJsonHelper jsonHelper,
            IViewComponentHelper viewComponentHelper,
            IUrlHelper urlHelper);
    }


    /// <summary>
    /// Provides the basic abstraction for registration.
    /// </summary>
    /// <typeparam name="TInputModel">The input model.</typeparam>
    /// <typeparam name="TOutputModel">The output model.</typeparam>
    public abstract class RegisterProviderBase<TInputModel, TOutputModel> : IRegisterProvider
        where TInputModel : class
        where TOutputModel : class
    {
        /// <inheritdoc />
        public abstract bool JuryOrContestant { get; }

        /// <inheritdoc />
        public abstract int Order { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Icon { get; }

        /// <inheritdoc cref="IRegisterProvider.CreateInputModelAsync(RegisterProviderContext)" />
        protected abstract Task<TInputModel> CreateInputModelAsync(RegisterProviderContext context);

        /// <inheritdoc cref="IRegisterProvider.ValidateAsync(RegisterProviderContext, object, ModelStateDictionary)" />
        protected abstract Task ValidateAsync(RegisterProviderContext context, TInputModel model, ModelStateDictionary modelState);

        /// <inheritdoc cref="IRegisterProvider.ExecuteAsync(RegisterProviderContext, object)" />
        protected abstract Task<TOutputModel> ExecuteAsync(RegisterProviderContext context, TInputModel model);

        /// <inheritdoc cref="IRegisterProvider.RenderInputAsync(RegisterProviderContext, RegisterProviderOutput)" />
        protected abstract Task RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput<TInputModel> output);

        /// <inheritdoc cref="IRegisterProvider.RenderOutputAsync(RegisterProviderContext, RegisterProviderOutput)" />
        protected abstract Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<TOutputModel> output);

        /// <inheritdoc cref="IRegisterProvider.CreateInputRenderer(ViewContext, object, IModelExpressionProvider, IJsonHelper, IViewComponentHelper, IUrlHelper)" />
        protected virtual RegisterProviderOutput<TInputModel> CreateInputRenderer(ViewContext viewContext, TInputModel model, IModelExpressionProvider modelExpressionProvider, IJsonHelper jsonHelper, IViewComponentHelper viewComponentHelper, IUrlHelper urlHelper)
            => new RegisterProviderOutput<TInputModel>(viewContext, model, modelExpressionProvider, jsonHelper, viewComponentHelper, urlHelper);

        /// <inheritdoc cref="IRegisterProvider.CreateOutputRenderer(ViewContext, object, IModelExpressionProvider, IJsonHelper, IViewComponentHelper, IUrlHelper)" />
        protected virtual RegisterProviderOutput<TOutputModel> CreateOutputRenderer(ViewContext viewContext, TOutputModel model, IModelExpressionProvider modelExpressionProvider, IJsonHelper jsonHelper, IViewComponentHelper viewComponentHelper, IUrlHelper urlHelper)
            => new RegisterProviderOutput<TOutputModel>(viewContext, model, modelExpressionProvider, jsonHelper, viewComponentHelper, urlHelper);

        /// <inheritdoc cref="IRegisterProvider.ReadAsync(object, ControllerBase)" />
        protected virtual Task<bool> ReadAsync(TInputModel model, ControllerBase controller)
            => controller.TryUpdateModelAsync(model);

        #region Implicit Implementations

        async Task<object> IRegisterProvider.CreateInputModelAsync(RegisterProviderContext context)
            => await CreateInputModelAsync(context);

        Task IRegisterProvider.ValidateAsync(RegisterProviderContext context, object model, ModelStateDictionary modelState)
            => ValidateAsync(context, (TInputModel)model, modelState);

        async Task<object> IRegisterProvider.ExecuteAsync(RegisterProviderContext context, object model)
            => await ExecuteAsync(context, (TInputModel)model);

        Task IRegisterProvider.RenderInputAsync(RegisterProviderContext context, RegisterProviderOutput output)
            => RenderInputAsync(context, (RegisterProviderOutput<TInputModel>)output);

        Task IRegisterProvider.RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput output)
            => RenderOutputAsync(context, (RegisterProviderOutput<TOutputModel>)output);

        RegisterProviderOutput IRegisterProvider.CreateInputRenderer(ViewContext a, object b, IModelExpressionProvider c, IJsonHelper d, IViewComponentHelper e, IUrlHelper f)
            => CreateInputRenderer(a, (TInputModel)b, c, d, e, f);

        RegisterProviderOutput IRegisterProvider.CreateOutputRenderer(ViewContext a, object b, IModelExpressionProvider c, IJsonHelper d, IViewComponentHelper e, IUrlHelper f)
            => CreateOutputRenderer(a, (TOutputModel)b, c, d, e, f);

        Task<bool> IRegisterProvider.ReadAsync(object model, ControllerBase controller)
            => ReadAsync((TInputModel)model, controller);

        #endregion
    }


    /// <summary>
    /// Provides the jury abstraction for registration.
    /// </summary>
    /// <typeparam name="TInputModel">The input model.</typeparam>
    /// <typeparam name="TOutputModel">The output model.</typeparam>
    public abstract class JuryRegisterProviderBase<TInputModel, TOutputModel> :
        RegisterProviderBase<TInputModel, TOutputModel>
        where TInputModel : class
        where TOutputModel : class
    {
        /// <inheritdoc />
        public override sealed bool JuryOrContestant => true;
    }


    /// <summary>
    /// Provides the contestant abstraction for registration.
    /// </summary>
    /// <typeparam name="TInputModel">The input model.</typeparam>
    public abstract class ContestantRegisterProviderBase<TInputModel> :
        RegisterProviderBase<TInputModel, StatusMessageModel>
        where TInputModel : class
    {
        /// <inheritdoc />
        public override sealed bool JuryOrContestant => false;

        /// <inheritdoc />
        protected override sealed RegisterProviderOutput<StatusMessageModel> CreateOutputRenderer(ViewContext viewContext, StatusMessageModel model, IModelExpressionProvider modelExpressionProvider, IJsonHelper jsonHelper, IViewComponentHelper viewComponentHelper, IUrlHelper urlHelper)
            => throw new System.NotSupportedException();

        /// <inheritdoc />
        protected override sealed Task RenderOutputAsync(RegisterProviderContext context, RegisterProviderOutput<StatusMessageModel> output)
            => throw new System.NotSupportedException();
    }
}
