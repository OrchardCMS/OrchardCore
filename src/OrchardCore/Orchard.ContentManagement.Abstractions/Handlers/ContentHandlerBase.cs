namespace Orchard.ContentManagement.Handlers
{
    public abstract class ContentHandlerBase : IContentHandler
    {
        public virtual void Activating(ActivatingContentContext context) { }
        public virtual void Activated(ActivatedContentContext context) { }
        public virtual void Initializing(InitializingContentContext context) { }
        public virtual void Initialized(InitializingContentContext context) { }
        public virtual void Creating(CreateContentContext context) { }
        public virtual void Created(CreateContentContext context) { }
        public virtual void Loading(LoadContentContext context) { }
        public virtual void Loaded(LoadContentContext context) { }
        public virtual void Updating(UpdateContentContext context) { }
        public virtual void Updated(UpdateContentContext context) { }
        public virtual void Versioning(VersionContentContext context) { }
        public virtual void Versioned(VersionContentContext context) { }
        public virtual void Publishing(PublishContentContext context) { }
        public virtual void Published(PublishContentContext context) { }
        public virtual void Unpublishing(PublishContentContext context) { }
        public virtual void Unpublished(PublishContentContext context) { }
        public virtual void Removing(RemoveContentContext context) { }
        public virtual void Removed(RemoveContentContext context) { }

        // TODO: Implement Clone event
        //protected virtual void Cloning(CloneContentContext context) { }
        //protected virtual void Cloned(CloneContentContext context) { }

        public virtual void GetContentItemAspect(ContentItemAspectContext context) { }
    }
}