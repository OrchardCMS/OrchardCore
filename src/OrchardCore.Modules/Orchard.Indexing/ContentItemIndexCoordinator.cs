using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;

namespace Orchard.Indexing
{
    public class ContentItemIndexCoordinator : IContentItemIndexHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentPartFactory _contentPartFactory;
        private readonly IEnumerable<IContentPartIndexHandler> _partIndexHandlers;
        private readonly IEnumerable<IContentFieldIndexHandler> _fieldIndexHandlers;

        public ContentItemIndexCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IContentPartFactory contentPartFactory,
            IEnumerable<IContentPartIndexHandler> partIndexHandlers,
            IEnumerable<IContentFieldIndexHandler> fieldIndexHandlers,
            ILogger<ContentItemIndexCoordinator> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartFactory = contentPartFactory;
            _partIndexHandlers = partIndexHandlers;
            _fieldIndexHandlers = fieldIndexHandlers;
            Logger = logger;
        }

        public ILogger Logger { get; }

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
                var partType = _contentPartFactory.GetContentPartType(partTypeName) ?? typeof(ContentPart);
                var part = (ContentPart)context.ContentItem.Get(partType, partName);

                var typePartIndexSettings = contentTypePartDefinition.GetSettings<ContentIndexSettings>();

                // Skip this part if it's not included in the index and it's not the default type part
                if (partName != partTypeName && !typePartIndexSettings.Included)
                {
                    continue;
                }

                await _partIndexHandlers.InvokeAsync(partIndexHandler => partIndexHandler.BuildIndexAsync(part, contentTypePartDefinition, context, typePartIndexSettings), Logger);

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    var partFieldIndexSettings = contentPartFieldDefinition.GetSettings<ContentIndexSettings>();

                    if (!partFieldIndexSettings.Included)
                    {
                        continue;
                    }

                    await _fieldIndexHandlers.InvokeAsync(_fieldIndexHandler => _fieldIndexHandler.BuildIndexAsync(part, contentTypePartDefinition, contentPartFieldDefinition, context, partFieldIndexSettings), Logger);
                }
            }

            return;
        }
    }
}
