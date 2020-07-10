using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
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

        private ShortcodeTemplatesDocument _shortcodeTemplatesDocument;

        public TemplateShortcodeProvider(
            ShortcodeTemplatesManager shortcodeTemplatesManager,
            ILiquidTemplateManager liquidTemplateManager)
        {
            _shortcodeTemplatesManager = shortcodeTemplatesManager;
            _liquidTemplateManager = liquidTemplateManager;
        }

        public async ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content)
        {
            _shortcodeTemplatesDocument ??= await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();
            if (!_shortcodeTemplatesDocument.ShortcodeTemplates.TryGetValue(identifier, out var template))
            {
                return null;
            }

            var model = new ShortcodeViewModel
            {
                Args = arguments,
                Content = content
            };

            return await _liquidTemplateManager.RenderAsync(template.Content, NullEncoder.Default, model,
                scope =>
                {
                    scope.SetValue("Content", model.Content);
                    scope.SetValue("Args", model.Args);
                });
        }
    }
}
