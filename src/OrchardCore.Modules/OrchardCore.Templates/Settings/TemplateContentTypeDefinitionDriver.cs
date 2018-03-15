using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Settings
{
    public class TemplateContentTypeDefinitionDriver : ContentTypeDefinitionDisplayDriver
    {
        private readonly IStringLocalizer<TemplateContentPartDefinitionDriver> S;

        public TemplateContentTypeDefinitionDriver(IStringLocalizer<TemplateContentPartDefinitionDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
            {
                var stereotype = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>().Stereotype;

                if (string.IsNullOrWhiteSpace(stereotype))
                {
                    stereotype = "Content";
                }

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        Key = $"{stereotype}__{contentTypeDefinition.Name}",
                        Description = S["Template for a {0} content item in detail views", contentTypeDefinition.DisplayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        Key = $"{stereotype}_Summary__{contentTypeDefinition.Name}",
                        Description = S["Template for a {0} content item in summary views", contentTypeDefinition.DisplayName]
                    });

            }).Location("Content");
        }
    }
}