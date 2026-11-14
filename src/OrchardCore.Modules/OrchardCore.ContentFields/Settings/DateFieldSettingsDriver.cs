using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class DateFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<DateField>
{
    internal readonly IStringLocalizer S;

    public DateFieldSettingsDriver(IStringLocalizer<DateFieldSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<DateFieldSettings>("DateFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<DateFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
        }).Location(l => l.Zone("Content").Card(S["Settings"], "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new DateFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
