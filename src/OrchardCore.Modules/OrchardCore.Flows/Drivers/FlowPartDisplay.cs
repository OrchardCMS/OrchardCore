using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
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
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
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

            return Shape<FlowPartViewModel>(hasItems ? "FlowPart" : "FlowPart_NoItems", m =>
            {
                m.FlowPart = flowPart;
                m.BuildPartDisplayContext = context;
            })
            .Location("Detail", hasItems ? "Content:5" : "-");
        }

        public override IDisplayResult Edit(FlowPart flowPart, BuildPartEditorContext context)
        {
            return Shape<FlowPartEditViewModel>("FlowPart_Edit", m =>
            {
                m.FlowPart = flowPart;
                m.Updater = context.Updater;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(FlowPart part, BuildPartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            var model = new FlowPartEditViewModel { FlowPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            part.Widgets.Clear();

            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentItem = _contentManager.New(model.ContentTypes[i]);

                contentItem.Weld(new FlowMetadata());

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, context.IsNew, htmlFieldPrefix: model.Prefixes[i]);

                part.Widgets.Add(contentItem);
            }

            return Edit(part, context);
        }
    }
}
