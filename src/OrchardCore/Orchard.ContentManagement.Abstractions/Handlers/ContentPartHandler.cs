using System;

namespace Orchard.ContentManagement.Handlers
{
    public abstract class ContentPartHandler<TPart> : IContentPartHandler where TPart : ContentPart, new()
    {
        void IContentPartHandler.Activated(ActivatedContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Activated(context, (TPart)part);
            }
        }

        void IContentPartHandler.Activating(ActivatingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Activating(context, (TPart)part);
            }
        }

        void IContentPartHandler.Initializing(InitializingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Initializing(context, (TPart)part);
            }
        }

        void IContentPartHandler.Initialized(InitializingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Initialized(context, (TPart)part);
            }
        }

        void IContentPartHandler.Creating(CreateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Creating(context, (TPart)part);
            }
        }

        void IContentPartHandler.Created(CreateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Created(context, (TPart)part);
            }
        }

        void IContentPartHandler.Loading(LoadContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Loading(context, (TPart)part);
            }
        }

        void IContentPartHandler.Loaded(LoadContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Loaded(context, (TPart)part);
            }
        }

        void IContentPartHandler.Updating(UpdateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Updating(context, (TPart)part);
            }
        }

        void IContentPartHandler.Updated(UpdateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Updated(context, (TPart)part);
            }
        }

        void IContentPartHandler.Versioning(VersionContentContext context, ContentPart existing, ContentPart building)
        {
            if (existing is TPart && building is TPart)
            {
                Versioning(context, (TPart)existing, (TPart)building);
            }
        }

        void IContentPartHandler.Versioned(VersionContentContext context, ContentPart existing, ContentPart building)
        {
            if (existing is TPart && building is TPart)
            {
                Versioned(context, (TPart)existing, (TPart)building);
            }
        }

        void IContentPartHandler.Publishing(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Publishing(context, (TPart)part);
            }
        }

        void IContentPartHandler.Published(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Published(context, (TPart)part);
            }
        }

        void IContentPartHandler.Unpublishing(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Unpublishing(context, (TPart)part);
            }
        }

        void IContentPartHandler.Unpublished(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Unpublished(context, (TPart)part);
            }
        }

        void IContentPartHandler.Removing(RemoveContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Removing(context, (TPart)part);
            }
        }

        void IContentPartHandler.Removed(RemoveContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                Removed(context, (TPart)part);
            }
        }

        void IContentPartHandler.GetContentItemAspect(ContentItemAspectContext context, ContentPart part)
        {
            if (part is TPart)
            {
                GetContentItemAspect(context, (TPart)part);
            }
        }

        public virtual void Activated(ActivatedContentContext context, TPart instance) { }
        public virtual void Activating(ActivatingContentContext context, TPart instance) { }
        public virtual void Initializing(InitializingContentContext context, TPart instance) { }
        public virtual void Initialized(InitializingContentContext context, TPart instance) { }
        public virtual void Creating(CreateContentContext context, TPart instance) { }
        public virtual void Created(CreateContentContext context, TPart instance) { }
        public virtual void Loading(LoadContentContext context, TPart instance) { }
        public virtual void Loaded(LoadContentContext context, TPart instance) { }
        public virtual void Updating(UpdateContentContext context, TPart instance) { }
        public virtual void Updated(UpdateContentContext context, TPart instance) { }
        public virtual void Versioning(VersionContentContext context, TPart existing, TPart building) { }
        public virtual void Versioned(VersionContentContext context, TPart existing, TPart building) { }
        public virtual void Publishing(PublishContentContext context, TPart instance) { }
        public virtual void Published(PublishContentContext context, TPart instance) { }
        public virtual void Unpublishing(PublishContentContext context, TPart instance) { }
        public virtual void Unpublished(PublishContentContext context, TPart instance) { }
        public virtual void Removing(RemoveContentContext context, TPart instance) { }
        public virtual void Removed(RemoveContentContext context, TPart instance) { }
        public virtual void GetContentItemAspect(ContentItemAspectContext context, TPart part) { }
    }
}