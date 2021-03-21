using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Settings
{
    public class PlacementContentPartFieldDefinitionDisplayDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly IStringLocalizer S;

        public PlacementContentPartFieldDefinitionDisplayDriver(IStringLocalizer<PlacementContentTypePartDefinitionDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition contentPartFieldDefinition)
        {
            return Initialize<ContentSettingsViewModel>("PlacementSettings", model =>
            {
                var shapeType = contentPartFieldDefinition.FieldDefinition.Name;
                var partName = contentPartFieldDefinition.PartDefinition.Name;
                var differentiator = $"{contentPartFieldDefinition.PartDefinition.Name}-{contentPartFieldDefinition.Name}";
                var displayName = contentPartFieldDefinition.DisplayName();

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentPart = partName,
                        Description = S["Placement for the {0} field in a {1}", displayName, partName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        DisplayType = "Detail",
                        ContentPart = partName,
                        Description = S["Placement for the {0} field in a {1} in detail views", displayName, partName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        DisplayType = "Summary",
                        ContentPart = partName,
                        Description = S["Placement for the {0} field in a {1} in summary views", displayName, partName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = $"{shapeType}_Edit",
                        Differentiator = differentiator,
                        DisplayType = "Edit",
                        ContentPart = partName,
                        Description = S["Placement in admin editor for the {0} field in a {1}", displayName, partName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        DisplayType = "SummaryAdmin",
                        ContentPart = partName,
                        Description = S["Placement in admin for summary views for the {0} field in a {1}", displayName, partName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        DisplayType = "DetailAdmin",
                        ContentPart = partName,
                        Description = S["Placement in admin for Detail views for the {0} field in a {1}", displayName, partName]
                    });

            }).Location("Shortcuts");
        }
    }
}
