using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;

namespace Ccs.Registration
{
    internal static class FluentTagExtensions
    {
        private static T NotNull<T>(T value, string name) where T : class
            => value ?? throw new ArgumentNullException(name);

        public static TagBuilder WithClass(this TagBuilder tagBuilder, string? className)
        {
            if (className == null) return tagBuilder;
            tagBuilder.AddCssClass(className);
            return tagBuilder;
        }

        public static TagBuilder AppendInner(this TagBuilder tagBuilder, string unencoded)
        {
            tagBuilder.InnerHtml.Append(NotNull(unencoded, nameof(unencoded)));
            return tagBuilder;
        }

        public static TagBuilder AppendInnerHtml(this TagBuilder tagBuilder, string encoded)
        {
            tagBuilder.InnerHtml.AppendHtml(NotNull(encoded, nameof(encoded)));
            return tagBuilder;
        }

        public static TagBuilder AppendInnerHtml(this TagBuilder tagBuilder, IHtmlContent content)
        {
            tagBuilder.InnerHtml.AppendHtml(NotNull(content, nameof(content)));
            return tagBuilder;
        }

        public static TagBuilder AppendInnerHtmlOrNot(this TagBuilder tagBuilder, IHtmlContent? content)
        {
            if (content == null) return tagBuilder;
            tagBuilder.InnerHtml.AppendHtml(content);
            return tagBuilder;
        }

        public static TagBuilder AppendList(this TagBuilder tagBuilder, IEnumerable<object> enumerable, Func<object, TagBuilder> writer)
        {
            tagBuilder.InnerHtml.AppendHtml(
                new EnumerableHtmlContent(
                    NotNull(enumerable, nameof(enumerable)),
                    NotNull(writer, nameof(writer))));
            return tagBuilder;
        }

        private class EnumerableHtmlContent : IHtmlContent
        {
            private readonly IEnumerable<object> _enumerable;
            private readonly Func<object, TagBuilder> _writer;

            public EnumerableHtmlContent(IEnumerable<object> enumerable, Func<object, TagBuilder> writer)
            {
                _enumerable = enumerable;
                _writer = writer;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                foreach (var item in _enumerable)
                {
                    var tr = _writer.Invoke(item);
                    tr.MergeAttribute("role", "row");
                    tr.WriteTo(writer, encoder);
                }
            }
        }
    }
}
