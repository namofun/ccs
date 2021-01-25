﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataTables.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ccs.Registration
{
    /// <summary>
    ///  Class used to represent the output of an <see cref="IRegisterProvider{TInputModel, TOutputModel}"/>.
    /// </summary>
    public class RegisterProviderOutput<TModel> : HtmlContentBuilder
    {
        /// <summary>
        /// The cache to store factories for HtmlContent
        /// </summary>
        private static readonly IMemoryCache FactoryCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new Microsoft.Extensions.Internal.SystemClock() });

        /// <summary>
        /// Provides view data.
        /// </summary>
        public ViewDataDictionary<TModel> ViewData { get; }

        /// <summary>
        /// Provides helps on HTML generating.
        /// </summary>
        public IHtmlGenerator Generator { get; }

        /// <summary>
        /// Provides model.
        /// </summary>
        public TModel Model => ViewData.Model;

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
        /// Initialize a <see cref="RegisterProviderOutput{TModel}"/>.
        /// </summary>
        public RegisterProviderOutput()
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Sets the dialog title.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        public RegisterProviderOutput<TModel> WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Appends a <c>&lt;div asp-validation-summary="All"&gt;&lt;/div&gt;</c>.
        /// </summary>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        public RegisterProviderOutput<TModel> AppendValidationSummary()
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
        /// <typeparam name="TElement">The target element type.</typeparam>
        /// <param name="for">The model expression.</param>
        /// <param name="items">The select option items.</param>
        /// <param name="comment">The comments.</param>
        /// <param name="required">Whether this field is required.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        public RegisterProviderOutput<TModel> AppendSelect<TElement>(
            Expression<Func<TModel, TElement>> @for,
            IEnumerable<SelectListItem> items,
            string? comment = null,
            bool required = true)
        {
            var asp_for = ModelExpressionProvider.CreateModelExpression(ViewData, @for);

            items ??= Enumerable.Empty<SelectListItem>();
            var realModelType = asp_for.ModelExplorer.ModelType;
            var allowMultiple = typeof(string) != realModelType && typeof(IEnumerable).IsAssignableFrom(realModelType);
            var currentValues = Generator.GetCurrentValues(ViewContext, asp_for.ModelExplorer, asp_for.Name, allowMultiple);

            var select = Generator.GenerateSelect(
                ViewContext,
                asp_for.ModelExplorer,
                optionLabel: null,
                expression: asp_for.Name,
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
                    .AppendInnerHtml(LableFor(asp_for))
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
        public RegisterProviderOutput<TModel> AppendTextArea(
            Expression<Func<TModel, string>> @for,
            string? comment = null,
            bool required = true)
        {
            var asp_for = ModelExpressionProvider.CreateModelExpression(ViewData, @for);

            var textarea = Generator.GenerateTextArea(
                ViewContext,
                asp_for.ModelExplorer,
                asp_for.Name,
                rows: 0,
                columns: 0,
                htmlAttributes: null);

            textarea.AddCssClass("form-control");
            textarea.MergeAttribute("style", "min-height:20em;");
            if (required) textarea.Attributes.Add("required", "required");

            return AppendHtml(
                new TagBuilder("div")
                    .WithClass("form-group")
                    .AppendInnerHtml(LableFor(asp_for))
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
        public RegisterProviderOutput<TModel> AppendDataTable<TElement>(
            IReadOnlyList<TElement> elements,
            string? tableClass = null,
            string? theadClass = null)
            where TElement : class
        {
            // Justification: the factory is implemented with Task.FromResult.
            var viewModel = FactoryCache.GetOrCreate(typeof(TElement),
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
        /// Marks this output only redirect to another action.
        /// </summary>
        /// <param name="action">The action name.</param>
        /// <param name="controller">The controller name.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The <see cref="IHtmlContentBuilder"/>.</returns>
        public RegisterProviderOutput<TModel> Redirect(
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
}
