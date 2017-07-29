using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.Views;
using Orchard.Flows.Models;
using Orchard.Flows.ViewModels;

namespace Orchard.Flows.Drivers
{
    public class BagPartDisplay : ContentPartDisplayDriver<BagPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;

        public BagPartDisplay(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public override IDisplayResult Display(BagPart bagPart, BuildPartDisplayContext context)
        {
            return Shape<BagPartViewModel>("BagPart", m =>
            {
                m.BagPart = bagPart;
                m.BuildPartDisplayContext = context;
            })
            .Location("Detail", "Content:5");
        }

        public override IDisplayResult Edit(BagPart bagPart, BuildPartEditorContext context)
        {
            return Shape<BagPartEditViewModel>("BagPart_Edit", m =>
            {
                m.BagPart = bagPart;
                m.Updater = context.Updater;
                m.ContainedContentTypeDefinitions = GetContainedContentTypes(bagPart);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(BagPart part, BuildPartEditorContext context)
        {
            var contentItemDisplayManager = _serviceProvider.GetRequiredService<IContentItemDisplayManager>();

            var model = new BagPartEditViewModel { BagPart = part };

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            part.ContentItems.Clear();

            for (var i = 0; i < model.Prefixes.Length; i++)
            {
                var contentItem = _contentManager.New(model.ContentTypes[i]);

                var widgetModel = await contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, htmlFieldPrefix: model.Prefixes[i]);

                part.ContentItems.Add(contentItem);
            }

            return Edit(part, context);
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(BagPart bagPart)
        {
            var settings = GetSettings(bagPart);
            var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();
            return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
        }

        private BagPartSettings GetSettings(BagPart bagPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(bagPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "BagPart", StringComparison.Ordinal));
            return contentTypePartDefinition.Settings.ToObject<BagPartSettings>();
        }
    }
}
