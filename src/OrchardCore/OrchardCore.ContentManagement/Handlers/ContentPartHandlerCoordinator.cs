using System.Collections.Generic;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Drivers.Coordinators
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

                await _partHandlers.InvokeAsync(async handler => await handler.ActivatingAsync(context, part), Logger);

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
                    await _partHandlers.InvokeAsync(async handler => await handler.ActivatedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.CreatingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.CreatedAsync(context, part), Logger);
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
                await _partHandlers.InvokeAsync(async handler => await handler.InitializingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.InitializedAsync(context, part), Logger);
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
                var part = context.ContentItem.Get<ContentPart>(typePartDefinition.Name);

                if (part == null)
                {
                    part = _contentPartFactory.GetTypeActivator(partName).CreateInstance();
                    context.ContentItem.Weld(typePartDefinition.Name, part);
                }

                await _partHandlers.InvokeAsync(async handler => await handler.LoadingAsync(context, part), Logger);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;

                    if (!part.Has(fieldName))
                    {
                        var activator = _contentFieldFactory.GetTypeActivator(partFieldDefinition.FieldDefinition.Name);
                        context.ContentItem.Get<ContentPart>(typePartDefinition.Name).Weld(fieldName, activator.CreateInstance());
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
                    await _partHandlers.InvokeAsync(async handler => await handler.LoadedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.PublishingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.PublishedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.RemovingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.RemovedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.UnpublishingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.UnpublishedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.UpdatingAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.UpdatedAsync(context, part), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.VersioningAsync(context, existingPart, buildingPart), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.VersionedAsync(context, existingPart, buildingPart), Logger);
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
                    await _partHandlers.InvokeAsync(async handler => await handler.GetContentItemAspectAsync(context, part), Logger);
                }
            }
        }
    }
}