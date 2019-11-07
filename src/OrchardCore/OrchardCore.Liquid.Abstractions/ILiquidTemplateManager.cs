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
        /// Renders a Liquid template on a <see cref="TextWriter"/>
        /// </summary>
        Task RenderAsync(string template, TextWriter textWriter, TextEncoder encoder, TemplateContext context);

        /// <summary>
        /// Renders a Liquid template as a <see cref="string"/>.
        /// </summary>
        Task<string> RenderAsync(string template, TextEncoder encoder, TemplateContext context);

        /// <summary>
        /// Validates a Liquid template.
        /// </summary>
        bool Validate(string template, out IEnumerable<string> errors);
    }

    public static class LiquidTemplateManagerExtensions
    {
        /// <summary>
        /// Renders a Liquid template containing HTML.
        /// </summary>
        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TemplateContext context)
        {
            return manager.RenderAsync(template, HtmlEncoder.Default, context);
        }
    }
}
