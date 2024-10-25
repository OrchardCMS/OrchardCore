using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Settings;

public sealed class FullTextAspectSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly ILiquidTemplateManager _templateManager;

    internal readonly IStringLocalizer S;

    public FullTextAspectSettingsDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<FullTextAspectSettingsDisplayDriver> localizer)
    {
        _templateManager = templateManager;
        S = localizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<FullTextAspectSettingsViewModel>("FullTextAspectSettings_Edit", model =>
        {
            var settings = contentTypeDefinition.GetSettings<FullTextAspectSettings>();

            model.IncludeFullTextTemplate = settings.IncludeFullTextTemplate;
            model.FullTextTemplate = settings.FullTextTemplate;
            model.IncludeDisplayText = settings.IncludeDisplayText;
            model.IncludeBodyAspect = settings.IncludeBodyAspect;
        }).Location("Content:6");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new FullTextAspectSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.IncludeFullTextTemplate,
            m => m.FullTextTemplate,
            m => m.IncludeDisplayText,
            m => m.IncludeBodyAspect);

        if (!string.IsNullOrEmpty(model.FullTextTemplate) && !_templateManager.Validate(model.FullTextTemplate, out var errors))
        {
            context.Updater.ModelState.AddModelError(
                nameof(model.FullTextTemplate),
                S["Full-text doesn't contain a valid Liquid expression. Details: {0}",
                string.Join(' ', errors)]);
        }
        else
        {
            context.Builder.WithSettings(new FullTextAspectSettings
            {
                IncludeFullTextTemplate = model.IncludeFullTextTemplate,
                FullTextTemplate = model.FullTextTemplate,
                IncludeDisplayText = model.IncludeDisplayText,
                IncludeBodyAspect = model.IncludeBodyAspect
            });
        }

        return Edit(contentTypeDefinition, context);
    }
}
