using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title.Settings;

public sealed class TitlePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<TitlePart>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public TitlePartSettingsDisplayDriver(
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<TitlePartSettingsDisplayDriver> localizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        S = localizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<TitlePartSettingsViewModel>("TitlePartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<TitlePartSettings>();

            model.Options = settings.Options;
            model.Pattern = settings.Pattern;
            model.RenderTitle = settings.RenderTitle;
            model.TitlePartSettings = settings;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new TitlePartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Pattern,
            m => m.Options,
            m => m.RenderTitle);

        if (model.Options == TitlePartOptions.GeneratedHidden || model.Options == TitlePartOptions.GeneratedDisabled)
        {
            if (string.IsNullOrWhiteSpace(model.Pattern))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["A pattern is required when using the selected behavior option."]);
            }
            else if (!_liquidTemplateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["The pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(' ', errors)]);
            }
        }

        context.Builder.WithSettings(new TitlePartSettings
        {
            Pattern = model.Pattern,
            Options = model.Options,
            RenderTitle = model.RenderTitle,
        });

        return Edit(contentTypePartDefinition, context);
    }
}
