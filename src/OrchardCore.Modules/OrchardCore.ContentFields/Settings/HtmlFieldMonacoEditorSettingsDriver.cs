using System;
using System.Text.Json;
using System.Threading.Tasks;
using Jint;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldMonacoEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
    {
        protected readonly IStringLocalizer S;

        public HtmlFieldMonacoEditorSettingsDriver(IStringLocalizer<HtmlFieldMonacoEditorSettingsDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MonacoSettingsViewModel>("HtmlFieldMonacoEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<HtmlFieldMonacoEditorSettings>();
                if (string.IsNullOrWhiteSpace(settings.Options))
                {
                    settings.Options = JConvert.SerializeObject(new { automaticLayout = true, language = "html" }, JOptions.Indented);
                }

                model.Options = settings.Options;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Monaco")
            {
                var model = new MonacoSettingsViewModel();
                var settings = new HtmlFieldMonacoEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                    var options = model.Options.Trim();

                    if (!options.StartsWith('{') || !options.EndsWith('}'))
                    {
                        throw new Exception();
                    }

                    var engine = new Engine()
                        .Execute("var config = " + options + "; config.language = 'html';");

                    var jsValue = engine.Evaluate("JSON.stringify(config, null, 4)");

                    settings.Options = jsValue.AsString();

                    context.Builder.WithSettings(settings);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Options), S["The options are written in an incorrect format."]);
                }
            }

            return Edit(partFieldDefinition, context.Updater);
        }
    }
}
