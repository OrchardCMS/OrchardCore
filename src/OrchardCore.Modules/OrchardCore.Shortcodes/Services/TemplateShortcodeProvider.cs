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
            if (!(_liquidTemplateManager.Context.LocalScope.GetValue(identifier) is NilValue))
            {
                return null;
            }

            var model = new ShortcodeViewModel
            {
                Args = arguments,
                Content = content,
                Context = context
            };

            return await _liquidTemplateManager.RenderAsync(template.Content, _htmlEncoder, model,
                scope =>
                {
                    // Used for recursion checking.
                    scope.SetValue(identifier, "");

                    var content = scope.GetValue("Content").ToObjectValue();
                    if (content is LiquidContentAccessor contentAccessor)
                    {
                        contentAccessor.Content = model.Content;
                        scope.SetValue("Content", contentAccessor);
                    }
                    else
                    {
                        scope.SetValue("Content", model.Content);
                    }

                    scope.SetValue("Args", model.Args);
                    scope.SetValue("Context", model.Context);
                });
        }
    }
}
