using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Settings;

public sealed class MarkdownFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MarkdownField>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public MarkdownFieldSettingsDriver(IStringLocalizer<MarkdownFieldSettingsDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<MarkdownFieldSettingsViewModel>("MarkdownFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MarkdownFieldSettings>();

            model.SanitizeHtml = settings.SanitizeHtml;
            model.Hint = settings.Hint;
        }).Location(l => l.Zone("Content", "20").Card(_localizerFactory.Localize(CommonLocalizationSources.Settings), "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new MarkdownFieldSettingsViewModel();
        var settings = new MarkdownFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.SanitizeHtml = model.SanitizeHtml;
        settings.Hint = model.Hint;

        context.Builder.WithSettings(settings);

        return Edit(partFieldDefinition, context);
    }
}
