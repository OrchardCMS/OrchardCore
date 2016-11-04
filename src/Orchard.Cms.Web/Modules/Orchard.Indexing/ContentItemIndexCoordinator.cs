using System.Collections.Generic;
using System.Threading.Tasks;
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
                var part = context.ContentItem.Get(partType, partName) as ContentPart;

                var typePartIndexSettings = contentTypePartDefinition.GetSettings<ContentIndexSettings>();

                await _partIndexHandlers.InvokeAsync(partIndexHandler => partIndexHandler.BuildIndexAsync(part, contentTypePartDefinition, context, typePartIndexSettings), Logger);

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    var partFieldIndexSettings = contentPartFieldDefinition.GetSettings<ContentIndexSettings>();

                    await _fieldIndexHandlers.InvokeAsync(_fieldIndexHandler => _fieldIndexHandler.BuildIndexAsync(part, contentTypePartDefinition, contentPartFieldDefinition, context, partFieldIndexSettings), Logger);
                }
            }

            return;
        }
    }
}
