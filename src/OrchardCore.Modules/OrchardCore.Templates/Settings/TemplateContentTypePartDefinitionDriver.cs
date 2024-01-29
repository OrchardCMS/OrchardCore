using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Settings
{
    public class TemplateContentTypePartDefinitionDriver : ContentTypePartDefinitionDisplayDriver
    {
        protected readonly IStringLocalizer S;

        public TemplateContentTypePartDefinitionDriver(IStringLocalizer<TemplateContentTypePartDefinitionDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
            {
                var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partName = contentTypePartDefinition.Name;

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        Key = $"{contentType}__{partName}",
                        Description = S["Template for the {0} part in a {1} type in detail views", partName, contentTypePartDefinition.ContentTypeDefinition.DisplayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        Key = $"{contentType}_Summary__{partName}",
                        Description = S["Template for the {0} part in a {1} type in summary views", partName, contentTypePartDefinition.ContentTypeDefinition.DisplayName]
                    });
            }).Location("Shortcuts");
        }
    }
}
