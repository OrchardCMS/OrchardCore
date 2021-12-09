using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Indexing
{
    /// <summary>
    /// Enumerates all parts and fields of content item to extract indexed properties.
    /// </summary>
    public class ContentItemIndexCoordinator : IContentItemIndexHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentPartIndexHandler> _partIndexHandlers;
        private readonly IEnumerable<IContentFieldIndexHandler> _fieldIndexHandlers;
        private readonly ILogger _logger;

        public ContentItemIndexCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentPartIndexHandler> partIndexHandlers,
            IEnumerable<IContentFieldIndexHandler> fieldIndexHandlers,
            ILogger<ContentItemIndexCoordinator> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartFactory = contentPartFactory;
            _partIndexHandlers = partIndexHandlers;
            _fieldIndexHandlers = fieldIndexHandlers;
            _logger = logger;
        }

        public async Task BuildIndexAsync(BuildIndexContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = (ContentPart)context.ContentItem.Get(partActivator.Type, partName);

                var typePartIndexSettings = contentTypePartDefinition.GetSettings<ContentIndexSettings>();

                // Skip this part if it's not included in the index and it's not the default type part
                if (contentTypeDefinition.Name != partTypeName && !typePartIndexSettings.Included)
                {
                    continue;
                }

                await _partIndexHandlers.InvokeAsync((handler, part, contentTypePartDefinition, context, typePartIndexSettings) =>
                    handler.BuildIndexAsync(part, contentTypePartDefinition, context, typePartIndexSettings),
                        part, contentTypePartDefinition, context, typePartIndexSettings, _logger);

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    var partFieldIndexSettings = contentPartFieldDefinition.GetSettings<ContentIndexSettings>();

                    if (!partFieldIndexSettings.Included)
                    {
                        continue;
                    }

                    await _fieldIndexHandlers.InvokeAsync((handler, part, contentTypePartDefinition, contentPartFieldDefinition, context, partFieldIndexSettings) =>
                        handler.BuildIndexAsync(part, contentTypePartDefinition, contentPartFieldDefinition, context, partFieldIndexSettings),
                            part, contentTypePartDefinition, contentPartFieldDefinition, context, partFieldIndexSettings, _logger);
                }
            }

            return;
        }
    }
}
