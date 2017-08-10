using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Views;
using Orchard.Templates.ViewModels;

namespace Orchard.Templates.Settings
{
    public class TemplateContentTypeSettingsDriver : ContentTypeDisplayDriver
    {
        private readonly IStringLocalizer<TemplateContentPartSettingsDriver> S;

        public TemplateContentTypeSettingsDriver(IStringLocalizer<TemplateContentPartSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Shape<ContentSettingsViewModel>("TemplateSettings", model =>
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

                return Task.CompletedTask;
            }).Location("Content");
        }
    }
}