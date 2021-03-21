using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Settings
{
    public class PlacementContentPartDefinitionDriver : ContentPartDefinitionDisplayDriver
    {
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IStringLocalizer S;
        public PlacementContentPartDefinitionDriver(
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IStringLocalizer<PlacementContentPartDefinitionDriver> localizer)
        {
            _contentPartFactory = contentPartFactory;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            var displayName = contentPartDefinition.DisplayName();
            var partName = contentPartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);

            var shapeType = partName;

            if (partActivator.Type == typeof(ContentPart))
            {
                shapeType = "ContentPart";
            }

            return Initialize<ContentSettingsViewModel>("PlacementSettings", model =>
            {
                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Description = S["Placement for a {0} part", displayName],
                        ContentPart = partName
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        DisplayType = "Detail",
                        Description = S["Placement for a {0} part in detail views", displayName],
                        ContentPart = partName
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        DisplayType = "Summary",
                        Description = S["Placement for a {0} part in summary views", displayName],
                        ContentPart = partName
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = $"{shapeType}_Edit",
                        Description = S["Placement in admin editor for a {0} part", displayName],
                        DisplayType = "Edit",
                        ContentPart = partName
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        DisplayType = "SummaryAdmin",
                        Description = S["Placement in admin summary views for a {0} part", displayName],
                        ContentPart = partName
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        DisplayType = "DetailAdmin",
                        Description = S["Placement in admin detail views for a {0} part", displayName],
                        ContentPart = partName
                    });

            }).Location("Shortcuts");
        }
    }
}
