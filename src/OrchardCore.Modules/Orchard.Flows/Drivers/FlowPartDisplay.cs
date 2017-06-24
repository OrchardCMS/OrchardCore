using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.Views;
using Orchard.Flows.Models;
using Orchard.Flows.ViewModels;

namespace Orchard.Flows.Drivers
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
            return Shape<FlowPartViewModel>("FlowPart", m =>
            {
                m.FlowPart = flowPart;
                m.BuildPartDisplayContext = context;
            })
            .Location("Detail", "Content:5");
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

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, htmlFieldPrefix: model.Prefixes[i]);

                part.Widgets.Add(contentItem);
            }

            return Edit(part, context);
        }
    }
}
