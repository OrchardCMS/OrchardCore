using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Widgets.Models;

namespace OrchardCore.Widgets.Settings;

public sealed class WidgetsListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<WidgetsListPart>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly char[] s_separator = [',', ' '];

    internal readonly IStringLocalizer S;

    public WidgetsListPartSettingsDisplayDriver(IStringLocalizer<WidgetsListPartSettingsDisplayDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<WidgetsListPartSettingsViewModel>("WidgetsPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

            model.Zones = string.Join(", ", settings.Zones);
            model.WidgetsListPartSettings = settings;
        }).Location(l => l.Zone("Content").Card(_localizerFactory.Localize(CommonLocalizationSources.Settings), "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new WidgetsListPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Zones);

        context.Builder.WithSettings(new WidgetsListPartSettings { Zones = (model.Zones ?? string.Empty).Split(s_separator, StringSplitOptions.RemoveEmptyEntries) });

        return Edit(contentTypePartDefinition, context);
    }
}
