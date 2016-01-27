using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.Drivers.Coordinators
{
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
            // This method is called on New()
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

        public override void Loading(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _drivers.Select(cpp => cpp.GetPartInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                if (!context.ContentItem.Has(partName))
                {

                    var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                    var part = partInfo != null
                        ? partInfo.Factory(typePartDefinition)
                        : new ContentPart();

                    context.ContentItem.Weld(partName, part);
                }
            }
        }
    }
}