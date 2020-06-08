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
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Drivers
{
    public class FlowPartDisplay : ContentPartDisplayDriver<FlowPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;

        public FlowPartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public override IDisplayResult Display(FlowPart flowPart, BuildPartDisplayContext context)
        {
            var hasItems = flowPart.Widgets.Any();

            return Initialize<FlowPartViewModel>(hasItems ? "FlowPart" : "FlowPart_Empty", m =>
            {
                m.FlowPart = flowPart;
                m.BuildPartDisplayContext = context;
            })
            .Location("Detail", "Content:5");
        }

        public override IDisplayResult Edit(FlowPart flowPart, BuildPartEditorContext context)
        {
            return Initialize<FlowPartEditViewModel>("FlowPart_Edit", m =>
            {
                m.FlowPart = flowPart;
                m.Updater = context.Updater;
                m.ContainedContentTypeDefinitions = GetContainedContentTypes(context.TypePartDefinition);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(FlowPart part, UpdatePartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            var model = new FlowPartEditViewModel { FlowPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            var contentItems = new List<ContentItem>();

            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentItem = await _contentManager.NewAsync(model.ContentTypes[i]);
                var existing = part.Widgets.FirstOrDefault(x => String.Equals(x.ContentItemId, model.Prefixes[i], StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    contentItem.ContentItemId = model.Prefixes[i];
                }

                contentItem.Weld(new FlowMetadata());

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);

                contentItems.Add(contentItem);
            }

            part.Widgets = contentItems;

            return Edit(part, context);
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ContentTypePartDefinition typePartDefinition)
        {
            var settings = typePartDefinition.GetSettings<FlowPartSettings>();

            if (settings.ContainedContentTypes == null || !settings.ContainedContentTypes.Any())
            {
                return _contentDefinitionManager.ListTypeDefinitions().Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget");
            }

            return settings.ContainedContentTypes
                .Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType))
                .Where(t => t.GetSettings<ContentTypeSettings>().Stereotype == "Widget");
        }
    }
}
