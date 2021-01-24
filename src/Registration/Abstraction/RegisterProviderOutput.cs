#nullable enable
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using System.Text.Encodings.Web;

namespace Ccs.Registration
{
    /// <summary>
    ///  Class used to represent the output of an <see cref="IRegisterProvider{TInputModel, TOutputModel}"/>.
    /// </summary>
    public class RegisterProviderOutput<TModel> : HtmlContentBuilder
    {
        /// <summary>
        /// Provides helps on HTML Form or Model building.
        /// </summary>
        public IHtmlHelper<TModel> Html { get; }

        /// <summary>
        /// Provides helps on HTML generating.
        /// </summary>
        public IHtmlGenerator Generator { get; }

        /// <summary>
        /// Provides information on view context.
        /// </summary>
        public ViewContext ViewContext { get; }

        /// <summary>
        /// Provides helps on JSON building.
        /// </summary>
        public IJsonHelper Json { get; }

        /// <summary>
        /// Provides helps on view component building.
        /// </summary>
        public IViewComponentHelper Component { get; }

        /// <summary>
        /// Provides helps on url generating.
        /// </summary>
        public IUrlHelper Url { get; }

        /// <summary>
        /// Provides helps on contest.
        /// </summary>
        public IContestContextAccessor Contest { get; }

        /// <summary>
        /// Gets or sets the output dialog title.
        /// </summary>
        public string Title { get; private set; } = "Register";

        /// <summary>
        /// Initialize a <see cref="RegisterProviderOutput{TModel}"/>.
        /// </summary>
        public RegisterProviderOutput(
            ViewContext viewContext,
            IHtmlGenerator htmlGenerator,
            ICompositeViewEngine viewEngine,
            IModelMetadataProvider metadataProvider,
            IViewBufferScope bufferScope,
            HtmlEncoder htmlEncoder,
            UrlEncoder urlEncoder,
            ModelExpressionProvider modelExpressionProvider,
            IJsonHelper jsonHelper,
            IViewComponentHelper viewComponentHelper,
            IUrlHelper urlHelper,
            IContestContextAccessor contestContextAccessor)
        {
            var htmlHelper = new HtmlHelper<TModel>(
                htmlGenerator,
                viewEngine,
                metadataProvider,
                bufferScope,
                htmlEncoder,
                urlEncoder,
                modelExpressionProvider);
            htmlHelper.Contextualize(viewContext);
            
            Html = htmlHelper;
            Generator = htmlGenerator;
            ViewContext = viewContext;
            Json = jsonHelper;
            Component = viewComponentHelper;
            Url = urlHelper;
            Contest = contestContextAccessor;
        }

        public new RegisterProviderOutput<TModel> Append(string unencoded)
        {
            base.Append(unencoded);
            return this;
        }

        public new RegisterProviderOutput<TModel> AppendHtml(IHtmlContent htmlContent)
        {
            base.AppendHtml(htmlContent);
            return this;
        }

        public new RegisterProviderOutput<TModel> AppendHtml(string encoded)
        {
            base.AppendHtml(encoded);
            return this;
        }

        public new RegisterProviderOutput<TModel> Clear()
        {
            base.Clear();
            return this;
        }

        public RegisterProviderOutput<TModel> WithTitle(string title)
        {
            Title = title;
            return this;
        }
    }
}
