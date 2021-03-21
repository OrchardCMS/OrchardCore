using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Settings
{
    public class PlacementContentTypePartDefinitionDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IStringLocalizer S;

        public PlacementContentTypePartDefinitionDriver(
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IStringLocalizer<PlacementContentTypePartDefinitionDriver> localizer)
        {
            _contentPartFactory = contentPartFactory;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            return Initialize<ContentSettingsViewModel>("PlacementSettings", model =>
            {
                var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partName = contentTypePartDefinition.Name;
                var displayName = contentTypePartDefinition.ContentTypeDefinition.DisplayName;

                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);

                var shapeType = partName;
                var differentiator = "";

                if (partActivator.Type == typeof(ContentPart) && partTypeName != contentTypePartDefinition.ContentTypeDefinition.Name)
                {
                    shapeType = "ContentPart";
                    differentiator = partName;
                }

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentType = contentType,
                        Description = S["Placement for the {0} part in a {1} type", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentType = contentType,
                        DisplayType = "Detail",
                        Description = S["Placement for the {0} part in a {1} type in detail views", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentType = contentType,
                        DisplayType = "Summary",
                        Description = S["Placement for the {0} part in a {1} type in summary views", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = $"{partName}_Edit",
                        Differentiator = differentiator,
                        ContentType = contentType,
                        DisplayType = "Edit",
                        Description = S["Placement in admin editor for the {0} part in a {1} type", partName, displayName]
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentType = contentType,
                        DisplayType = "SummaryAdmin",
                        Description = S["Placement in admin for summary views for the {0} part in a {1} type", partName, displayName]                        
                    });

                model.ContentSettingsEntries.Add(
                    new ContentSettingsEntry
                    {
                        ShapeType = shapeType,
                        Differentiator = differentiator,
                        ContentType = contentType,
                        DisplayType = "DetailAdmin",
                        Description = S["Placement in admin for Detail views the {0} part in a {1} type", partName, displayName]                        
                    });

            }).Location("Shortcuts");
        }
    }
}
