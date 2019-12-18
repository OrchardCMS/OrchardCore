using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// This component coordinates how parts are affecting content items.
    /// </summary>
    public class ContentPartHandlerCoordinator : ContentHandlerBase
    {
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentPartHandler> _partHandlers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;

        public ContentPartHandlerCoordinator(
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentPartHandler> partHandlers,
            ITypeActivatorFactory<ContentField> contentFieldFactory,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentPartHandlerCoordinator> logger)
        {
            _contentPartFactory = contentPartFactory;
            _contentFieldFactory = contentFieldFactory;
            _partHandlers = partHandlers;
            _contentDefinitionManager = contentDefinitionManager;

            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public override async Task ActivatingAsync(ActivatingContentContext context)
        {
            // This method is called on New()
            // Adds all the parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;

                // We create the part from it's known type or from a generic one
                var part = _contentPartFactory.GetTypeActivator(partName).CreateInstance();

                await _partHandlers.InvokeAsync((handler, context, part) => handler.ActivatingAsync(context, part), context, part, Logger);

                context.Builder.Weld(typePartDefinition.Name, part);
            }
        }

        public override async Task ActivatedAsync(ActivatedContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, partName) as ContentPart;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.ActivatedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task CreatingAsync(CreateContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.CreatingAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task CreatedAsync(CreateContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.CreatedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task InitializingAsync(InitializingContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);

                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;
                await _partHandlers.InvokeAsync((handler, context, part) => handler.InitializingAsync(context, part), context, part, Logger);
            }
        }

        public override async Task InitializedAsync(InitializingContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.InitializedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task LoadingAsync(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.
            // A part is missing if the content type is changed and an old content item is loaded, 
            // like edited.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);

                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;

                // If no existing part was not found in the content item, create a new one
                if (part == null)
                {
                    part = activator.CreateInstance();
                    context.ContentItem.Weld(typePartDefinition.Name, part);
                }

                await _partHandlers.InvokeAsync((handler, context, part) => handler.LoadingAsync(context, part), context, part, Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;

                    if (!part.Has(fieldName))
                    {
                        var fieldActivator = _contentFieldFactory.GetTypeActivator(partFieldDefinition.FieldDefinition.Name);
                        context.ContentItem.Get<ContentPart>(typePartDefinition.Name).Weld(fieldName, fieldActivator.CreateInstance());
                    }
                }
            }
        }

        public override async Task LoadedAsync(LoadContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.LoadedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.PublishingAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.PublishedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task RemovingAsync(RemoveContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.RemovingAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.RemovedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task UnpublishingAsync(PublishContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.UnpublishingAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.UnpublishedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task UpdatingAsync(UpdateContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.UpdatingAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task UpdatedAsync(UpdateContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart; ;

                if (part != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.UpdatedAsync(context, part), context, part, Logger);
                }
            }
        }

        public override async Task VersioningAsync(VersionContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);

                var buildingPart = context.BuildingContentItem.Get(activator.Type, partName) as ContentPart;
                var existingPart = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;

                if (buildingPart != null && existingPart != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, existingPart, buildingPart) => handler.VersioningAsync(context, existingPart, buildingPart), context, existingPart, buildingPart, Logger);
                }
            }
        }

        public override async Task VersionedAsync(VersionContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);

                var buildingPart = (ContentPart)context.BuildingContentItem.Get(activator.Type, partName);
                var existingPart = (ContentPart)context.ContentItem.Get(activator.Type, typePartDefinition.Name);

                if (buildingPart != null && existingPart != null)
                {
                    await _partHandlers.InvokeAsync((handler, context, existingPart, buildingPart) => handler.VersionedAsync(context, existingPart, buildingPart), context, existingPart, buildingPart, Logger);
                }
            }
        }

        public override async Task GetContentItemAspectAsync(ContentItemAspectContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.GetContentItemAspectAsync(context, part), context, part, Logger);
                }
            }
        }
        public override async Task ClonedAsync(CloneContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.ClonedAsync(context, part), context, part, Logger);
                }
            }
        }
        public override async Task CloningAsync(CloneContentContext context)
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
                    await _partHandlers.InvokeAsync((handler, context, part) => handler.CloningAsync(context, part), context, part, Logger);
                }
            }
        }

    }
}
