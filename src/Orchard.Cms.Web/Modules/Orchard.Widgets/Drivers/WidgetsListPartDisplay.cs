using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Modules.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.Views;
using Orchard.Settings;
using Orchard.Widgets.Models;
using Orchard.Widgets.Settings;
using Orchard.Widgets.ViewModels;

namespace Orchard.Widgets.Drivers
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
            return Shape<WidgetsListPartEditViewModel>("WidgetsListPart_Edit", m =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(widgetPart.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(WidgetsListPart));
                var settings = contentTypePartDefinition.GetSettings<WidgetsListPartSettings>();

                m.AvailableZones = settings.Zones;
                
                m.WidgetsListPart = widgetPart;
                m.Updater = context.Updater;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(WidgetsListPart part, BuildPartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            var model = new WidgetsListPartEditViewModel { WidgetsListPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            part.Widgets.Clear();

            // Remove any content or the zones would be merged and not be cleared
            part.Content.Widgets.RemoveAll();

            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentType = model.ContentTypes[i];
                var zone = model.Zones[i];
                var prefix = model.Prefixes[i];

                var contentItem = _contentManager.New(contentType);

                contentItem.Weld(new WidgetMetadata());

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, htmlFieldPrefix: prefix);

                if (!part.Widgets.ContainsKey(zone))
                {
                    part.Widgets.Add(zone, new List<ContentItem>());
                }

                part.Widgets[zone].Add(contentItem);
            }

            return Edit(part, context);
        }
    }
}
