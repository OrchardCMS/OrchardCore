using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Settings;

public sealed class TemplateContentPartDefinitionDriver : ContentPartDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    public TemplateContentPartDefinitionDriver(IStringLocalizer<TemplateContentPartDefinitionDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition, BuildEditorContext context)
    {
        return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
        {
            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = contentPartDefinition.Name,
                    Description = S["Template for a {0} part in detail views", contentPartDefinition.DisplayName()]
                });

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{contentPartDefinition.Name}_Summary",
                    Description = S["Template for a {0} part in summary views", contentPartDefinition.DisplayName()]
                });
        }).Location("Shortcuts");
    }
}
