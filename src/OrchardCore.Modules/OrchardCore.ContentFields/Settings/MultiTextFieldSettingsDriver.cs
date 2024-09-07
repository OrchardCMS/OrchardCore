using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class MultiTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MultiTextField>
{
    internal readonly IStringLocalizer S;

    public MultiTextFieldSettingsDriver(IStringLocalizer<MultiTextFieldSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<MultiTextFieldSettingsViewModel>("MultiTextFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MultiTextFieldSettings>();

            model.Required = settings.Required;
            model.Hint = settings.Hint;
            model.Options = JConvert.SerializeObject(settings.Options, JOptions.Indented);
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new MultiTextFieldSettingsViewModel();
        var settings = new MultiTextFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.Required = model.Required;
        settings.Hint = model.Hint;

        try
        {
            settings.Options = JConvert.DeserializeObject<MultiTextFieldValueOption[]>(model.Options);

            context.Builder.WithSettings(settings);
        }
        catch
        {
            context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
        }

        return Edit(partFieldDefinition, context);
    }
}
