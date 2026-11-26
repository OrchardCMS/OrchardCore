using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.ContentFields.Settings;

public sealed class BooleanFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<BooleanField>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public BooleanFieldSettingsDriver(IStringLocalizer<BooleanFieldSettingsDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<BooleanFieldSettings>("BooleanFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<BooleanFieldSettings>();

            model.Hint = settings.Hint;
            model.Label = settings.Label;
            model.DefaultValue = settings.DefaultValue;
        }).Location(l => l.Zone("Content").Card(_localizerFactory.Localize(CommonLocalizationSources.Settings), "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new BooleanFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
