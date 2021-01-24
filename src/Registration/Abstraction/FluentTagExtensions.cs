using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Ccs.Registration
{
    internal static class FluentTagExtensions
    {
        private static T NotNull<T>(T value, string name) where T : class
            => value ?? throw new ArgumentNullException(name);

        public static TagBuilder WithClass(this TagBuilder tagBuilder, string className)
        {
            tagBuilder.AddCssClass(NotNull(className, nameof(className)));
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

        public static TagBuilder AppendInnerHtmlOrNot(this TagBuilder tagBuilder, IHtmlContent content)
        {
            if (content == null) return tagBuilder;
            tagBuilder.InnerHtml.AppendHtml(content);
            return tagBuilder;
        }
    }
}
