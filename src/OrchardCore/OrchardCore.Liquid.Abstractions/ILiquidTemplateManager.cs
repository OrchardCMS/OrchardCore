using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;

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
        Task<string> RenderAsync(string template, TextEncoder encoder, object model, Action<LiquidTemplateContext> action);

        /// <summary>
        /// Renders a Liquid template on a <see cref="TextWriter"/>.
        /// </summary>
        Task RenderAsync(string template, TextWriter writer, TextEncoder encoder, object model, Action<LiquidTemplateContext> action);

        /// <summary>
        /// Validates a Liquid template.
        /// </summary>
        bool Validate(string template, out IEnumerable<string> errors);
    }

    public static class LiquidTemplateManagerExtensions
    {
        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder)
            => manager.RenderAsync(template, encoder, null, action: null);

        public static Task RenderAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder)
            => manager.RenderAsync(template, writer, encoder, null, action: null);

        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, object model)
            => manager.RenderAsync(template, encoder, model, action: null);

        public static Task RenderAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder, object model)
            => manager.RenderAsync(template, writer, encoder, model, action: null);

        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, Action<LiquidTemplateContext> action)
            => manager.RenderAsync(template, encoder, null, action);

        public static Task RenderAsync(this ILiquidTemplateManager manager, string template, TextWriter writer, TextEncoder encoder, Action<LiquidTemplateContext> action)
            => manager.RenderAsync(template, writer, encoder, null, action);
    }
}
