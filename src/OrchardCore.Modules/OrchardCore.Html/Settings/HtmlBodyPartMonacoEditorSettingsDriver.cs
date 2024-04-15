using System;
using System.Text.Json;
using System.Threading.Tasks;
using Jint;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartMonacoEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver<HtmlBodyPart>
    {
        protected readonly IStringLocalizer S;

        public HtmlBodyPartMonacoEditorSettingsDriver(IStringLocalizer<HtmlBodyPartMonacoEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<MonacoSettingsViewModel>("HtmlBodyPartMonacoSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartMonacoEditorSettings>();
                if (string.IsNullOrWhiteSpace(settings.Options))
                {
                    settings.Options = JConvert.SerializeObject(new { automaticLayout = true, language = "html" }, JOptions.Indented);
                }
                model.Options = settings.Options;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (contentTypePartDefinition.Editor() == "Monaco")
            {
                var model = new MonacoSettingsViewModel();
                var settings = new HtmlBodyPartMonacoEditorSettings();

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

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
