using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Drivers.Coordinators
{
    /// <summary>
    /// This component coordinates how parts are affecting content items.
    /// </summary>
    public class ContentPartHandlerCoordinator : ContentHandlerBase
    {
        private readonly IContentPartHandlerResolver _contentPartHandlerResolver;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentPartHandler> _partHandlers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;

        public ContentPartHandlerCoordinator(
            IContentPartHandlerResolver contentPartHandlerResolver,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentPartHandler> partHandlers,
            ITypeActivatorFactory<ContentField> contentFieldFactory,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentPartHandlerCoordinator> logger)
        {
            _contentPartHandlerResolver = contentPartHandlerResolver;
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
                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                // For backward compatability check for Any().
                // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                if (partHandlers != null && partHandlers.Any())
                {
                    await partHandlers.InvokeAsync(handler => handler.ActivatingAsync(context, part), Logger);
                }
                // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                else
                {
                    await _partHandlers.InvokeAsync(handler => handler.ActivatingAsync(context, part), Logger);
                }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.ActivatedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.ActivatedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.CreatingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.CreatingAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.CreatedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.CreatedAsync(context, part), Logger);
                    }
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
                if (part != null)
                {
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.InitializingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.InitializingAsync(context, part), Logger);
                    }
                }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.InitializedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.InitializedAsync(context, part), Logger);
                    }
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

                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                // For backward compatability check for Any().
                // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                if (partHandlers != null && partHandlers.Any())
                {
                    await partHandlers.InvokeAsync(handler => handler.LoadingAsync(context, part), Logger);
                }
                // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                else
                {
                    await _partHandlers.InvokeAsync(handler => handler.LoadingAsync(context, part), Logger);
                }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.LoadedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.LoadedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.PublishingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(async handler => {
                            Logger.LogWarning("The handler '{Handler}' should not be registerd as IContentPartHandler. Use WithHandler<T> instead.", handler.GetType());
                            await handler.PublishingAsync(context, part);
                        }, Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.PublishedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.PublishedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.RemovingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.RemovingAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.RemovedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.RemovedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.UnpublishingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.UnpublishingAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.UnpublishedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.UnpublishedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.UpdatingAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.UpdatingAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.UpdatedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.UpdatedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.VersioningAsync(context, existingPart, buildingPart), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.VersioningAsync(context, existingPart, buildingPart), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.VersionedAsync(context, existingPart, buildingPart), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.VersionedAsync(context, existingPart, buildingPart), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.GetContentItemAspectAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.GetContentItemAspectAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.ClonedAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.ClonedAsync(context, part), Logger);
                    }
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
                    var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                    // For backward compatability check for Any().
                    // TODO: Any() can be removed in a future release as the recommended way is to use ContentOptions.
                    if (partHandlers != null && partHandlers.Any())
                    {
                        await partHandlers.InvokeAsync(handler => handler.CloningAsync(context, part), Logger);
                    }
                    // TODO: This can be removed in a future release as the recommended way is to use ContentOptions.
                    else
                    {
                        await _partHandlers.InvokeAsync(handler => handler.CloningAsync(context, part), Logger);
                    }
                }
            }
        }

    }
}