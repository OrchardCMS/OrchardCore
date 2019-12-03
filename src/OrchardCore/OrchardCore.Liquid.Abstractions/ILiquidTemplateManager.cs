using System.Collections.Generic;
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
        /// Gets the current <see cref="TemplateContext"/>.
        /// </summary>
        TemplateContext Context { get; }

        /// <summary>
        /// Renders a Liquid template as a <see cref="string"/>.
        /// </summary>
        Task<string> RenderAsync(string template, TextEncoder encoder, object model);

        /// <summary>
        /// Validates a Liquid template.
        /// </summary>
        bool Validate(string template, out IEnumerable<string> errors);
    }

    public static class LiquidTemplateManagerExtensions
    {
        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder)
            => manager.RenderAsync(template, encoder, null);
    }
}
