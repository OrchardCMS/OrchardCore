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
using OrchardCore.Settings;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Drivers
{
    public class WidgetsListPartDisplay : ContentPartDisplayDriver<WidgetsListPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteService _siteService;

        public WidgetsListPartDisplay(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider,
            ISiteService siteService
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _siteService = siteService;
        }

        public override async Task<IDisplayResult> DisplayAsync(WidgetsListPart part, BuildPartDisplayContext context)
        {
            if (context.DisplayType != "Detail" || !part.Widgets.Any())
            {
                return null;
            }

            dynamic layout = context.Layout;
            dynamic layoutZones = layout.Zones;

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
                        contentZone.Add(widgetContent);
                    }
                }
            }

            return null;
        }

        public override IDisplayResult Edit(WidgetsListPart widgetPart, BuildPartEditorContext context)
        {
            return Initialize<WidgetsListPartEditViewModel>("WidgetsListPart_Edit", m =>
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
                    var existing = part.Widgets[zone].FirstOrDefault(x => String.Equals(x.ContentItemId, model.Prefixes[i], StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        contentItem.ContentItemId = model.Prefixes[i];
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
