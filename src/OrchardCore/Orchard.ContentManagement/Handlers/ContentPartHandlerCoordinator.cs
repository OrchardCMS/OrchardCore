using System.Collections.Generic;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers.Coordinators
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

        public override void Activating(ActivatingContentContext context)
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

                _partHandlers.Invoke(handler => handler.Activating(context, part), Logger);

                context.Builder.Weld(typePartDefinition.Name, part);
            }
        }

        public override void Activated(ActivatedContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Activated(context, part), Logger);
                }
            }
        }

        public override void Creating(CreateContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Creating(context, part), Logger);
                }
            }
        }

        public override void Created(CreateContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Created(context, part), Logger);
                }
            }
        }

        public override void Initializing(InitializingContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);

                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;
                _partHandlers.Invoke(handler => handler.Initializing(context, part), Logger);
            }
        }

        public override void Initialized(InitializingContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Initialized(context, part), Logger);
                }
            }
        }

        public override void Loading(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                if (!context.ContentItem.Has(partName))
                {
                    var part = _contentPartFactory.GetTypeActivator(partName).CreateInstance();
                    _partHandlers.Invoke(handler => handler.Loading(context, part), Logger);
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var part = context.ContentItem.Get<ContentPart>(typePartDefinition.Name);
                    var fieldName = partFieldDefinition.Name;

                    if (!part.Has(fieldName))
                    {
                        var activator = _contentFieldFactory.GetTypeActivator(partFieldDefinition.FieldDefinition.Name);
                        context.ContentItem.Get<ContentPart>(typePartDefinition.Name).Weld(fieldName, activator.CreateInstance());
                    }
                }
            }
        }

        public override void Loaded(LoadContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Loaded(context, part), Logger);
                }
            }
        }

        public override void Publishing(PublishContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Publishing(context, part), Logger);
                }
            }
        }

        public override void Published(PublishContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Published(context, part), Logger);
                }
            }
        }

        public override void Removing(RemoveContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Removing(context, part), Logger);
                }
            }
        }

        public override void Removed(RemoveContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Removed(context, part), Logger);
                }
            }
        }

        public override void Unpublishing(PublishContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Unpublishing(context, part), Logger);
                }
            }
        }

        public override void Unpublished(PublishContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Unpublished(context, part), Logger);
                }
            }
        }

        public override void Updating(UpdateContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Updating(context, part), Logger);
                }
            }
        }

        public override void Updated(UpdateContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Updated(context, part), Logger);
                }
            }
        }

        public override void Versioning(VersionContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Versioning(context, existingPart, buildingPart), Logger);
                }
            }
        }

        public override void Versioned(VersionContentContext context)
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
                    _partHandlers.Invoke(handler => handler.Versioned(context, existingPart, buildingPart), Logger);
                }
            }
        }
        
        public override void GetContentItemAspect(ContentItemAspectContext context)
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
                    _partHandlers.Invoke(handler => handler.GetContentItemAspect(context, part), Logger);
                }
            }
        }
    }
}