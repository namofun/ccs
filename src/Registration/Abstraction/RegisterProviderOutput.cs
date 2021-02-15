using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataTables.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ccs.Registration
{
    /// <summary>
    /// Base class used to represent the output of an <see cref="IRegisterProvider"/>.
    /// </summary>
    public abstract class RegisterProviderOutput : HtmlContentBuilder
    {
        /// <summary>
        /// The cache to store factories for HtmlContent
        /// </summary>
        private static readonly IMemoryCache DataTableFactoryCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new Microsoft.Extensions.Internal.SystemClock() });

        /// <summary>
        /// Provides view data.
        /// </summary>
        public ViewDataDictionary ViewData { get; }

        /// <summary>
        /// Provides helps on HTML generating.
        /// </summary>
        public IHtmlGenerator Generator { get; }

        /// <summary>
        /// Provides model.
        /// </summary>
        public object Model => ViewData.Model;

        /// <summary>
        /// Provides <see cref="ModelExpression"/> for expressions.
        /// </summary>
        public IModelExpressionProvider ModelExpressionProvider { get; }

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
        /// Gets or sets the output dialog title.
        /// </summary>
        public string Title { get; private set; } = "Register";

        /// <summary>
        /// Initialize a <see cref="RegisterProviderOutput"/>.
        /// </summary>
        protected RegisterProviderOutput(
            ViewDataDictionary viewData,
            IHtmlGenerator htmlGenerator,
            IModelExpressionProvider modelExpressionProvider,
            ViewContext originalViewContext,
            IJsonHelper jsonHelper,
            IViewComponentHelper viewComponentHelper,
            IUrlHelper urlHelper)
        {
            ViewData = viewData;
            Generator = htmlGenerator;
            ModelExpressionProvider = modelExpressionProvider;
            ViewContext = new ViewContext(originalViewContext, originalViewContext.View, viewData, originalViewContext.Writer);
            Json = jsonHelper;
            Component = viewComponentHelper;
            Url = urlHelper;
        }

        private TagBuilder LableFor(ModelExpression @for)
            => Generator.GenerateLabel(
                ViewContext,
                @for.ModelExplorer,
                @for.Name,
                labelText: null,
                htmlAttributes: null);

        private TagBuilder? CommentFor(string? comment)
            => comment == null ? null : new TagBuilder("small")
                .WithClass("text-muted mt-2")
                .AppendInner(comment);

        /// <summary>
        /// Sets the dialog title.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Appends a <c>&lt;div asp-validation-summary="All"&gt;&lt;/div&gt;</c>.
        /// </summary>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder AppendValidationSummary()
        {
            var tagBuilder = Generator.GenerateValidationSummary(
                ViewContext,
                excludePropertyErrors: false,
                message: null,
                headerTag: null,
                htmlAttributes: null);

            if (tagBuilder == null)
            {
                // The generator determined no element was necessary.
                return this;
            }
            else
            {
                tagBuilder.AddCssClass("text-danger");
                return AppendHtml(tagBuilder);
            }
        }

        /// <summary>
        /// Appends a <c>&lt;select&gt;</c> element.
        /// </summary>
        /// <param name="for">The model expression.</param>
        /// <param name="items">The select option items.</param>
        /// <param name="comment">The comments.</param>
        /// <param name="required">Whether this field is required.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder AppendSelect(
            ModelExpression @for,
            IEnumerable<SelectListItem> items,
            string? comment = null,
            bool required = true)
        {
            items ??= Enumerable.Empty<SelectListItem>();
            var realModelType = @for.ModelExplorer.ModelType;
            var allowMultiple = typeof(string) != realModelType && typeof(IEnumerable).IsAssignableFrom(realModelType);
            var currentValues = Generator.GetCurrentValues(ViewContext, @for.ModelExplorer, @for.Name, allowMultiple);

            var select = Generator.GenerateSelect(
                ViewContext,
                @for.ModelExplorer,
                optionLabel: null,
                expression: @for.Name,
                selectList: items,
                currentValues: currentValues,
                allowMultiple: allowMultiple,
                htmlAttributes: null);

            if (select == null)
            {
                throw new InvalidOperationException("Generation for select got wrong.");
            }

            select.AddCssClass("form-control custom-select");
            if (required) select.Attributes.Add("required", "required");

            return AppendHtml(
                new TagBuilder("div")
                    .WithClass("form-group")
                    .AppendInnerHtml(LableFor(@for))
                    .AppendInnerHtml(select)
                    .AppendInnerHtmlOrNot(CommentFor(comment)));
        }

        /// <summary>
        /// Appends a <c>&lt;textarea&gt;</c> element.
        /// </summary>
        /// <param name="for">The model expression.</param>
        /// <param name="comment">The comments.</param>
        /// <param name="required">Whether this field is required.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder AppendTextArea(
            ModelExpression @for,
            string? comment = null,
            bool required = true)
        {
            var textarea = Generator.GenerateTextArea(
                ViewContext,
                @for.ModelExplorer,
                @for.Name,
                rows: 0,
                columns: 0,
                htmlAttributes: null);

            textarea.AddCssClass("form-control");
            textarea.MergeAttribute("style", "min-height:20em;");
            if (required) textarea.Attributes.Add("required", "required");

            return AppendHtml(
                new TagBuilder("div")
                    .WithClass("form-group")
                    .AppendInnerHtml(LableFor(@for))
                    .AppendInnerHtml(textarea)
                    .AppendInnerHtmlOrNot(CommentFor(comment)));
        }

        /// <summary>
        /// Appends a <c>&lt;table&gt;</c> element.
        /// </summary>
        /// <typeparam name="TElement">The element types.</typeparam>
        /// <param name="elements">The inner elements.</param>
        /// <param name="tableClass">The attached class for table.</param>
        /// <param name="theadClass">The attached class for thead.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder AppendDataTable<TElement>(
            IReadOnlyList<TElement> elements,
            string? tableClass = null,
            string? theadClass = null)
            where TElement : class
        {
            // Justification: the factory is implemented with Task.FromResult.
            var viewModel = DataTableFactoryCache.GetOrCreate(typeof(TElement),
                entry => DataRowFunctions.Factory((Type)entry.Key)).Result;

            var uniqueId = Guid.NewGuid().ToString()[0..6];

            var tbody = new TagBuilder("tbody")
                .AppendList(elements, viewModel.TRow);

            var thead = new TagBuilder("thead")
                .AppendInnerHtml(viewModel.THead)
                .WithClass(theadClass);

            var table = new TagBuilder("table")
                .WithClass("data-table table table-sm table-striped")
                .WithClass(tableClass)
                .AppendInnerHtml(thead)
                .AppendInnerHtml(tbody);

            var script = new TagBuilder("script");
            script.InnerHtml
                .AppendHtmlLine("$().ready(function(){$('#" + uniqueId + "').DataTable({")
                .AppendHtmlLine("'searching': " + (viewModel.Searchable ? "true" : "false") + ",")
                .AppendHtmlLine("'ordering': " + (viewModel.Sortable ? ("true, 'order': [" + viewModel.Sort + "],") : "false,"))
                .AppendHtmlLine("'paging': false, 'info': false, 'autoWidth': false,")
                .AppendHtmlLine("'language': { 'searchPlaceholder': 'filter table', 'search': '_INPUT_', 'oPaginate': {'sPrevious': '&laquo;', 'sNext': '&raquo;'} },")
                .AppendHtmlLine("'aoColumnDefs': [{ aTargets: ['sortable'], bSortable: true }, { aTargets: ['searchable'], bSearchable: true }, { aTargets: ['_all'], bSortable: false, bSearchable: false }],")
                .AppendHtmlLine("});});");

            table.MergeAttribute("id", uniqueId);
            base.AppendHtml(table);
            base.AppendHtml(script);
            return this;
        }

        /// <summary>
        /// Appends a &lt;div class="alert"&gt; element.
        /// </summary>
        /// <param name="content">The alert content.</param>
        /// <param name="color">The alert color.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder AppendAlert(
            string content,
            BootstrapColor color = BootstrapColor.success)
        {
            var alert = new TagBuilder("div");
            alert.AppendInner(content);
            alert.AddCssClass("alert alert-" + color);
            base.AppendHtml(alert);
            return this;
        }

        /// <summary>
        /// Marks this output only redirect to another action.
        /// </summary>
        /// <param name="action">The action name.</param>
        /// <param name="controller">The controller name.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        protected IHtmlContentBuilder Redirect(
            string? action = null,
            string? controller = null,
            object? routeValues = null)
        {
            var destinationUrl = Url.Action(action, controller, routeValues);
            var request = ViewContext.HttpContext.Request;
            var response = ViewContext.HttpContext.Response;

            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new InvalidOperationException("No Routes Matched");
            }

            if (request.IsAjax())
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Headers["X-Login-Page"] = destinationUrl;
            }
            else
            {
                response.StatusCode = StatusCodes.Status302Found;
                response.Headers[HeaderNames.Location] = destinationUrl;
            }

            return this;
        }
    }


    /// <summary>
    /// Strongly typed class used to represent the output of an <see cref="IRegisterProvider"/>.
    /// </summary>
    public class RegisterProviderOutput<TModel> : RegisterProviderOutput where TModel : class
    {
        private ModelExpression Create<TElement>(Expression<Func<TModel, TElement>> @for)
            => ModelExpressionProvider.CreateModelExpression(ViewData, @for);

        private static ViewDataDictionary Create(ViewDataDictionary viewData, TModel model, string prefix = "")
        {
            var retVal = new ViewDataDictionary<TModel>(viewData, model);
            retVal.TemplateInfo.HtmlFieldPrefix = prefix;
            return retVal;
        }

        private static IHtmlGenerator GetHtmlGenerator(HttpContext context)
            => context.RequestServices.GetRequiredService<IHtmlGenerator>();

        /// <summary>
        /// Initialize a <see cref="RegisterProviderOutput{TModel}"/>.
        /// </summary>
        public RegisterProviderOutput(
            ViewContext viewContext,
            TModel model,
            IModelExpressionProvider modelExpressionProvider,
            IJsonHelper jsonHelper,
            IViewComponentHelper viewComponentHelper,
            IUrlHelper urlHelper,
            string prefix = "")
            : base(Create(viewContext.ViewData, model, prefix),
                  GetHtmlGenerator(viewContext.HttpContext),
                  modelExpressionProvider,
                  viewContext,
                  jsonHelper,
                  viewComponentHelper,
                  urlHelper)
        {
        }

        /// <inheritdoc cref="RegisterProviderOutput.ViewData"/>
        public new ViewDataDictionary<TModel> ViewData => (ViewDataDictionary<TModel>)base.ViewData;

        /// <inheritdoc cref="RegisterProviderOutput.Model"/>
        public new TModel Model => ViewData.Model;

        /// <inheritdoc cref="IHtmlContentBuilder.Append(string)" />
        public new RegisterProviderOutput<TModel> Append(string unencoded)
        {
            base.Append(unencoded);
            return this;
        }

        /// <inheritdoc cref="IHtmlContentBuilder.AppendHtml(IHtmlContent)" />
        public new RegisterProviderOutput<TModel> AppendHtml(IHtmlContent htmlContent)
        {
            base.AppendHtml(htmlContent);
            return this;
        }

        /// <inheritdoc cref="IHtmlContentBuilder.Append(string)" />
        public new RegisterProviderOutput<TModel> AppendHtml(string encoded)
        {
            base.AppendHtml(encoded);
            return this;
        }

        /// <inheritdoc cref="IHtmlContentBuilder.Clear" />
        public new RegisterProviderOutput<TModel> Clear()
        {
            base.Clear();
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.WithTitle(string)" />
        public new RegisterProviderOutput<TModel> WithTitle(string title)
        {
            base.WithTitle(title);
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.AppendValidationSummary" />
        public new RegisterProviderOutput<TModel> AppendValidationSummary()
        {
            base.AppendValidationSummary();
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.AppendSelect(ModelExpression, IEnumerable{SelectListItem}, string?, bool)" />
        public RegisterProviderOutput<TModel> AppendSelect<TElement>(Expression<Func<TModel, TElement>> @for, IEnumerable<SelectListItem> items, string? comment = null, bool required = true)
        {
            base.AppendSelect(Create(@for), items, comment, required);
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.AppendTextArea(ModelExpression, string?, bool)" />
        public RegisterProviderOutput<TModel> AppendTextArea(Expression<Func<TModel, string>> @for, string? comment = null, bool required = true)
        {
            base.AppendTextArea(Create(@for), comment, required);
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.AppendDataTable{TElement}(IReadOnlyList{TElement}, string?, string?)" />
        public new RegisterProviderOutput<TModel> AppendDataTable<TElement>(IReadOnlyList<TElement> elements, string? tableClass = null, string? theadClass = null) where TElement : class
        {
            base.AppendDataTable(elements, tableClass, theadClass);
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.AppendAlert(string, BootstrapColor)"/>
        public new IHtmlContentBuilder AppendAlert(string content, BootstrapColor color = BootstrapColor.success)
        {
            base.AppendAlert(content, color);
            return this;
        }

        /// <inheritdoc cref="RegisterProviderOutput.Redirect(string?, string?, object?)" />
        public new RegisterProviderOutput<TModel> Redirect(string? action = null, string? controller = null, object? routeValues = null)
        {
            base.Redirect(action, controller, routeValues);
            return this;
        }
    }
}
