using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Widgets.Models;

namespace OrchardCore.Widgets.Settings;

public sealed class WidgetsListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<WidgetsListPart>
{
    private static readonly char[] _separator = [',', ' '];

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<WidgetsListPartSettingsViewModel>("WidgetsPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

            model.Zones = string.Join(", ", settings.Zones);
            model.WidgetsListPartSettings = settings;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new WidgetsListPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Zones);

        context.Builder.WithSettings(new WidgetsListPartSettings { Zones = (model.Zones ?? string.Empty).Split(_separator, StringSplitOptions.RemoveEmptyEntries) });

        return Edit(contentTypePartDefinition, context);
    }
}
