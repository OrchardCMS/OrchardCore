using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Views;
using Orchard.Templates.ViewModels;

namespace Orchard.Templates.Settings
{
    public class TemplateContentPartSettingsDriver : ContentPartDisplayDriver
    {
        private readonly IStringLocalizer<TemplateContentPartSettingsDriver> S;

        public TemplateContentPartSettingsDriver(IStringLocalizer<TemplateContentPartSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            return Shape<ContentSettingsViewModel>("TemplateSettings", model =>
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

                return Task.CompletedTask;
            }).Location("Content");
        }
    }
}