using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.ViewModels;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class TemplateShortcodeProvider : IShortcodeProvider
    {
        private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly HashSet<string> _identifiers = new HashSet<string>();

        private ShortcodeTemplatesDocument _shortcodeTemplatesDocument;

        public TemplateShortcodeProvider(
            ShortcodeTemplatesManager shortcodeTemplatesManager,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _shortcodeTemplatesManager = shortcodeTemplatesManager;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public async ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            _shortcodeTemplatesDocument ??= await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();
            if (!_shortcodeTemplatesDocument.ShortcodeTemplates.TryGetValue(identifier, out var template))
            {
                return null;
            }

            // Check if a shortcode template is recursively called.
            if (_identifiers.Contains(identifier))
            {
                return null;
            }
            else
            {
                _identifiers.Add(identifier);
            }

            var model = new ShortcodeViewModel
            {
                Args = arguments,
                Content = content,
                Context = context
            };

            var parameters = new Dictionary<string, FluidValue>();
            parameters[identifier] = new StringValue("");

            // TODO: Fix 'Content' property conflict differently, see #8259

            // var c = context.GetValue("Content").ToObjectValue();
            // if (c is LiquidContentAccessor contentAccessor)
            // {
            //     contentAccessor.Content = model.Content ?? "";
            //     parameters["Content"] = contentAccessor;
            // }
            // else
            // {
            //     parameters["Content"] = model.Content ?? "";
            // }

            parameters["Args"] = new ObjectValue(model.Args);
            parameters["Content"] = new StringValue(model.Content);
            parameters["Context"] = new ObjectValue(model.Context);

            return await _liquidTemplateManager.RenderStringAsync(template.Content, _htmlEncoder, model, parameters);
        }
    }
}
