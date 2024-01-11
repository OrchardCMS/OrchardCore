using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Settings
{
    public class PlacementContentPartDefinitionDriver : ContentPartDefinitionDisplayDriver
    {
        protected readonly IStringLocalizer S;

        public PlacementContentPartDefinitionDriver(IStringLocalizer<PlacementContentPartDefinitionDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            var displayName = contentPartDefinition.DisplayName();

            return Initialize<ContentSettingsViewModel>("PlacementSettings", model =>
            {
                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = contentPartDefinition.Name,
                        Description = S["Placement for a {0} part", displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = contentPartDefinition.Name,
                        DisplayType = "Detail",
                        Description = S["Placement for a {0} part in detail views", displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = contentPartDefinition.Name,
                        DisplayType = "Summary",
                        Description = S["Placement for a {0} part in summary views", displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = $"{contentPartDefinition.Name}_Edit",
                        Description = S["Placement in admin editor for a {0} part", displayName]
                    });

            }).Location("Shortcuts");
        }
    }
}
