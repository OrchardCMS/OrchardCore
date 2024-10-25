using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Settings;

public sealed class FacebookPluginPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<FacebookPluginPart>
{
    private readonly ILiquidTemplateManager _templateManager;

    internal readonly IStringLocalizer S;

    public FacebookPluginPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<FacebookPluginPartSettingsDisplayDriver> localizer)
    {
        _templateManager = templateManager;
        S = localizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<FacebookPluginPartSettingsViewModel>("FacebookPluginPartSettings_Edit", model =>
        {
            model.FacebookPluginPartSettings = contentTypePartDefinition.GetSettings<FacebookPluginPartSettings>();
            model.Liquid = model.FacebookPluginPartSettings.Liquid;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new FacebookPluginPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Liquid);

        if (!string.IsNullOrEmpty(model.Liquid) && !_templateManager.Validate(model.Liquid, out var errors))
        {
            context.Updater.ModelState.AddModelError(nameof(model.Liquid), S["The Body doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
        }
        else
        {
            context.Builder.WithSettings(new FacebookPluginPartSettings { Liquid = model.Liquid });
        }

        return Edit(contentTypePartDefinition, context);
    }
}
