using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Settings;

public sealed class MarkdownBodyPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<MarkdownBodyPart>
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public MarkdownBodyPartSettingsDisplayDriver(IStringLocalizer<MarkdownBodyPartSettingsDisplayDriver> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<MarkdownBodyPartSettingsViewModel>("MarkdownBodyPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

            model.SanitizeHtml = settings.SanitizeHtml;
        }).Location(l => l.Zone("Content", "20").Card(_localizerFactory.Localize(CommonLocalizationSources.Settings), "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new MarkdownBodyPartSettingsViewModel();
        var settings = new MarkdownBodyPartSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.SanitizeHtml = model.SanitizeHtml;

        context.Builder.WithSettings(settings);

        return Edit(contentTypePartDefinition, context);
    }
}
