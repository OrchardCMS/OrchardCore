using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.Liquid
{
    /// <summary>
    /// Provides services to render Liquid templates.
    /// </summary>
    public interface ILiquidTemplateManager
    {
        /// <summary>
        /// Renders a Liquid template as a <see cref="string"/>.
        /// </summary>
        Task<string> RenderStringAsync(string template, TextEncoder encoder, object model, IEnumerable<KeyValuePair<string, FluidValue>> properties);

        /// <summary>
        /// Renders a Liquid template as a <see cref="IHtmlContent"/>.
        /// </summary>
        Task<IHtmlContent> RenderHtmlContentAsync(string template, TextEncoder encoder, object model, IEnumerable<KeyValuePair<string, FluidValue>> properties);

        /// <summary>
        /// Renders a Liquid template on a <see cref="TextWriter"/>.
        /// </summary>
        Task RenderAsync(string template, TextWriter writer, TextEncoder encoder, object model, IEnumerable<KeyValuePair<string, FluidValue>> properties);

        /// <summary>
        /// Validates a Liquid template.
        /// </summary>
        bool Validate(string template, out IEnumerable<string> errors);
    }

    public static class LiquidTemplateManagerExtensions
    {
        public static Task<string> RenderStringAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder)
            => manager.RenderStringAsync(template, encoder, model: null, properties: null);

        public static Task RenderStringAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder)
            => manager.RenderAsync(template, writer, encoder, model: null, properties: null);

        public static Task<string> RenderStringAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, object model)
            => manager.RenderStringAsync(template, encoder, model, properties: null);

        public static Task RenderStringAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder, object model)
            => manager.RenderAsync(template, writer, encoder, model, properties: null);

        public static Task<string> RenderStringAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, IEnumerable<KeyValuePair<string, FluidValue>> properties)
            => manager.RenderStringAsync(template, encoder, model: null, properties);

        public static Task RenderStringAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder, IEnumerable<KeyValuePair<string, FluidValue>> properties)
            => manager.RenderAsync(template, writer, encoder, model: null, properties);

        public static Task<IHtmlContent> RenderHtmlContentAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder)
            => manager.RenderHtmlContentAsync(template, encoder, model: null, properties: null);

        public static Task<IHtmlContent> RenderHtmlContentAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, object model)
            => manager.RenderHtmlContentAsync(template, encoder, model, properties: null);

        public static Task<IHtmlContent> RenderHtmlContentAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, IEnumerable<KeyValuePair<string, FluidValue>> properties)
            => manager.RenderHtmlContentAsync(template, encoder, model: null, properties);
    }
}
