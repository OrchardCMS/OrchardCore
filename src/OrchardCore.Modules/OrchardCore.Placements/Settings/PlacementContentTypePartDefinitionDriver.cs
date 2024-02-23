using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Settings
{
    public class PlacementContentTypePartDefinitionDriver : ContentTypePartDefinitionDisplayDriver
    {
        protected readonly IStringLocalizer S;

        public PlacementContentTypePartDefinitionDriver(IStringLocalizer<PlacementContentTypePartDefinitionDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            return Initialize<ContentSettingsViewModel>("PlacementSettings", model =>
            {
                var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partName = contentTypePartDefinition.Name;
                var displayName = contentTypePartDefinition.ContentTypeDefinition.DisplayName;

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = partName,
                        ContentType = contentType,
                        Description = S["Placement for the {0} part in a {1} type", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = partName,
                        ContentType = contentType,
                        DisplayType = "Detail",
                        Description = S["Placement for the {0} part in a {1} type in detail views", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = partName,
                        ContentType = contentType,
                        DisplayType = "Summary",
                        Description = S["Placement for the {0} part in a {1} type in summary views", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = $"{partName}_Edit",
                        ContentType = contentType,
                        Description = S["Placement in admin editor for the {0} part in a {1} type", partName, displayName]
                    });

            }).Location("Shortcuts");
        }
    }
}
