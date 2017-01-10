using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Flows.Model;
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

        public override IDisplayResult Display(FlowPart flowPart)
        {
            return Combine(
                Shape<FlowPartEditViewModel>("FlowPart", m => BuildViewModel(m, flowPart))
                    .Location("Detail", "Content:5")
            );
        }

        public override IDisplayResult Edit(FlowPart flowPart)
        {
            return Shape<FlowPartEditViewModel>("FlowPart_Edit", m => BuildViewModel(m, flowPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(FlowPart model, IUpdateModel updater)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            //await updater.TryUpdateModelAsync(model, Prefix);
            for (int i=0; i<3; i++)
            {
                var contentItem = _contentManager.New("Quote");

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, updater, htmlFieldPrefix: "FlowPart" + "." + i);
            }

            return Edit(model);
        }
        
        private FlowPartEditViewModel BuildViewModel(FlowPartEditViewModel model, FlowPart flowPart)
        {
            return new FlowPartEditViewModel();
        }
    }
}
