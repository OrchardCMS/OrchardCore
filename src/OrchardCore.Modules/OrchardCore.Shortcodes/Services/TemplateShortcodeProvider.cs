using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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

        private ShortcodeTemplatesDocument _shortcodeTemplatesDocument;
        private readonly HashSet<string> _identifiers = new();

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
                Context = context,
            };

            var parameters = new Dictionary<string, FluidValue>
            {
                [identifier] = new StringValue(""),
                ["Args"] = new ObjectValue(model.Args),
                ["Content"] = new ObjectValue(new Content(model.Content)),
                ["Context"] = new ObjectValue(model.Context)
            };

            var result = await _liquidTemplateManager.RenderStringAsync(template.Content, _htmlEncoder, model, parameters);

            // Allow multiple serial calls of this shortcode template.
            _identifiers.Remove(identifier);

            return result;
        }

        internal class Content : LiquidContentAccessor
        {
            public readonly string _content;
            public Content(string content) => _content = content;
            public override string ToString() => _content;
        }
    }
}
