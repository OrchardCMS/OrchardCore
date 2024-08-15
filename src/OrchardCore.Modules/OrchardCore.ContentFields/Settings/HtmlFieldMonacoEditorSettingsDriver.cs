using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentFields.Settings;

public sealed class HtmlFieldMonacoEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
{
    internal readonly IStringLocalizer S;

    public HtmlFieldMonacoEditorSettingsDriver(IStringLocalizer<HtmlFieldMonacoEditorSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<MonacoSettingsViewModel>("HtmlFieldMonacoEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<HtmlFieldMonacoEditorSettings>();
            if (string.IsNullOrWhiteSpace(settings.Options))
            {
                settings.Options = JConvert.SerializeObject(new
                {
                    automaticLayout = true,
                    language = "html"
                }, JOptions.Indented);
            }

            model.Options = settings.Options;
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "Monaco")
        {
            var model = new MonacoSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (!model.Options.IsJson())
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Options), S["The options are written in an incorrect format."]);
            }
            else
            {
                var jsonSettings = JObject.Parse(model.Options);
                jsonSettings["language"] = "html";
                var settings = new HtmlFieldMonacoEditorSettings
                {
                    Options = jsonSettings.ToString()
                };
                context.Builder.WithSettings(settings);
            }
        }

        return Edit(partFieldDefinition, context);
    }
}
