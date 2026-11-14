using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class TextFieldHeaderDisplaySettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
{
    internal readonly IStringLocalizer S;

    public TextFieldHeaderDisplaySettingsDriver(IStringLocalizer<TextFieldHeaderDisplaySettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<HeaderSettingsViewModel>("TextFieldHeaderDisplaySettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TextFieldHeaderDisplaySettings>();

            model.Level = settings.Level;
        }).Location(l => l.Zone("DisplayMode").Card(S["Display mode"], "60"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.DisplayMode() == "Header")
        {
            var model = new HeaderSettingsViewModel();
            var settings = new TextFieldHeaderDisplaySettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.Level = model.Level;

            context.Builder.WithSettings(settings);
        }

        return Edit(partFieldDefinition, context);
    }
}
