using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentLocalization.Handlers
{
    class ContentLocalizationPartHandlerCoordinator : ContentLocalizationHandlerBase
    {
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentLocalizationPartHandler> _partHandlers;
        private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILogger<ContentLocalizationPartHandlerCoordinator> _logger;

        public ContentLocalizationPartHandlerCoordinator(

            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentLocalizationPartHandler> partHandlers,
            ITypeActivatorFactory<ContentField> contentFieldFactory,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentLocalizationPartHandlerCoordinator> logger
        )
        {
            _contentPartFactory = contentPartFactory;
            _partHandlers = partHandlers;
            _contentFieldFactory = contentFieldFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _logger = logger;
        }

        public override async Task LocalizingAsync(LocalizationContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync(handler => handler.LocalizingAsync(context, part), _logger);
                }
            }
        }

        public override async Task LocalizedAsync(LocalizationContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync(handler => handler.LocalizedAsync(context, part), _logger);
                }
            }
        }
    }
}
