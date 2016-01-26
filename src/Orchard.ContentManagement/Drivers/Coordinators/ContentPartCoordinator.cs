using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Microsoft.Extensions.Logging;
using System;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    /// <summary>
    /// This component coordinates how parts are affecting content items.
    /// </summary>
    public class ContentPartCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _drivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPartCoordinator(
            IEnumerable<IContentPartDriver> drivers, 
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentPartCoordinator> logger) {
            _drivers = drivers;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public override void Activating(ActivatingContentContext context)
        {
            // Adds all the parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _drivers.Select(cpp => cpp.GetPartInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                var part = partInfo != null
                    ? partInfo.Factory(typePartDefinition)
                    : new ContentPart();

                context.Builder.Weld(partName, part);
            }
        }

        protected void Apply(ContentItem contentItem, Action<ContentElement> action)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var part = contentItem.Get(partName);

                action(part);
            }
        }

        public override void GetContentItemMetadata(ContentItemMetadataContext context)
        {
            Apply(context.ContentItem, part => _drivers.Invoke(driver => driver.GetContentItemMetadata(part, context), Logger));
        }

    }
}