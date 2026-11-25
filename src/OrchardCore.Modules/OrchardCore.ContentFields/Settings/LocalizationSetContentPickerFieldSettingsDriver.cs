using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Settings;

[RequireFeatures("OrchardCore.ContentLocalization")]
public sealed class LocalizationSetContentPickerFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<LocalizationSetContentPickerField>
{
    internal readonly IStringLocalizer S;

    public LocalizationSetContentPickerFieldSettingsDriver(IStringLocalizer<LocalizationSetContentPickerFieldSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<LocalizationSetContentPickerFieldSettings>("LocalizationSetContentPickerFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<LocalizationSetContentPickerFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.Multiple = settings.Multiple;
            model.DisplayedContentTypes = settings.DisplayedContentTypes;
        }).Location(l => l.Zone("Content").Card(S["Settings"], "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new LocalizationSetContentPickerFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
