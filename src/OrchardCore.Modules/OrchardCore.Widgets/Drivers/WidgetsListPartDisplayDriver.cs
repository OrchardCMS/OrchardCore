using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Drivers
{
    public class WidgetsListPartDisplayDriver : ContentPartDisplayDriver<WidgetsListPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;

        public WidgetsListPartDisplayDriver(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public override async Task<IDisplayResult> DisplayAsync(WidgetsListPart part, BuildPartDisplayContext context)
        {
            if (context.DisplayType != "Detail" || !part.Widgets.Any())
            {
                return null;
            }

            var layout = context.Layout;
            var layoutZones = layout.Zones;

            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            foreach (var zone in part.Widgets.Keys)
            {
                foreach (var widget in part.Widgets[zone])
                {
                    var layerMetadata = widget.As<WidgetMetadata>();

                    if (layerMetadata != null)
                    {
                        var widgetContent = await contentItemDisplayManager.BuildDisplayAsync(widget, context.Updater);

                        widgetContent.Classes.Add("widget");
                        widgetContent.Classes.Add("widget-" + widget.ContentItem.ContentType.HtmlClassify());

                        var contentZone = layoutZones[zone];
                        await contentZone.AddAsync(widgetContent, "");
                    }
                }
            }

            return null;
        }

        public override IDisplayResult Edit(WidgetsListPart widgetPart, BuildPartEditorContext context)
        {
            return Initialize<WidgetsListPartEditViewModel>(GetEditorShapeType(context), m =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(widgetPart.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(WidgetsListPart));
                var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

                m.AvailableZones = settings.Zones;

                m.WidgetsListPart = widgetPart;
                m.Updater = context.Updater;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(WidgetsListPart part, UpdatePartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            var model = new WidgetsListPartEditViewModel { WidgetsListPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            var zonedContentItems = new Dictionary<string, List<ContentItem>>();

            // Remove any content or the zones would be merged and not be cleared
            part.Content.Widgets.RemoveAll();

            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentType = model.ContentTypes[i];
                var zone = model.Zones[i];
                var prefix = model.Prefixes[i];

                var contentItem = await _contentManager.NewAsync(contentType);
                if (part.Widgets.ContainsKey(zone))
                {
                    var existingContentItem = part.Widgets[zone].FirstOrDefault(x => String.Equals(x.ContentItemId, model.ContentItems[i], StringComparison.OrdinalIgnoreCase));
                    // When the content item already exists merge its elements to preverse nested content item ids.
                    // All of the data for these merged items is then replaced by the model values on update, while a nested content item id is maintained.
                    // This prevents nested items which rely on the content item id, i.e. the media attached field, losing their reference point.
                    if (existingContentItem != null)
                    {
                        contentItem.ContentItemId = model.ContentItems[i];
                        contentItem.Merge(existingContentItem);
                    }
                }

                contentItem.Weld(new WidgetMetadata());

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: prefix);

                if (!zonedContentItems.ContainsKey(zone))
                {
                    zonedContentItems.Add(zone, new List<ContentItem>());
                }

                zonedContentItems[zone].Add(contentItem);
            }

            part.Widgets = zonedContentItems;

            return Edit(part, context);
        }
    }
}
