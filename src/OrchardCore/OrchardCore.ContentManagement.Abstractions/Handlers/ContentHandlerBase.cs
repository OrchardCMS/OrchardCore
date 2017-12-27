using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    public abstract class ContentHandlerBase : IContentHandler
    {
        public virtual void Activating(ActivatingContentContext context) { }
        public virtual Task ActivatingAsync(ActivatingContentContext context)
        {
            Activating(context);
            return Task.CompletedTask;
        }

        public virtual void Activated(ActivatedContentContext context) { }
        public virtual Task ActivatedAsync(ActivatedContentContext context)
        {
            Activated(context);
            return Task.CompletedTask;
        }

        public virtual void Initializing(InitializingContentContext context) { }
        public virtual Task InitializingAsync(InitializingContentContext context)
        {
            Initializing(context);
            return Task.CompletedTask;
        }

        public virtual void Initialized(InitializingContentContext context) { }
        public virtual Task InitializedAsync(InitializingContentContext context)
        {
            Initialized(context);
            return Task.CompletedTask;
        }

        public virtual void Creating(CreateContentContext context) { }
        public virtual Task CreatingAsync(CreateContentContext context)
        {
            Creating(context);
            return Task.CompletedTask;
        }

        public virtual void Created(CreateContentContext context) { }
        public virtual Task CreatedAsync(CreateContentContext context)
        {
            Created(context);
            return Task.CompletedTask;
        }

        public virtual void Loading(LoadContentContext context) { }
        public virtual Task LoadingAsync(LoadContentContext context)
        {
            Loading(context);
            return Task.CompletedTask;
        }

        public virtual void Loaded(LoadContentContext context) { }
        public virtual Task LoadedAsync(LoadContentContext context)
        {
            Loaded(context);
            return Task.CompletedTask;
        }

        public virtual void Updating(UpdateContentContext context) { }
        public virtual Task UpdatingAsync(UpdateContentContext context)
        {
            Updating(context);
            return Task.CompletedTask;
        }

        public virtual void Updated(UpdateContentContext context) { }
        public virtual Task UpdatedAsync(UpdateContentContext context)
        {
            Updated(context);
            return Task.CompletedTask;
        }

        public virtual void Versioning(VersionContentContext context) { }
        public virtual Task VersioningAsync(VersionContentContext context)
        {
            Versioning(context);
            return Task.CompletedTask;
        }

        public virtual void Versioned(VersionContentContext context) { }
        public virtual Task VersionedAsync(VersionContentContext context)
        {
            Versioned(context);
            return Task.CompletedTask;
        }

        public virtual void Publishing(PublishContentContext context) { }
        public virtual Task PublishingAsync(PublishContentContext context)
        {
            Publishing(context);
            return Task.CompletedTask;
        }

        public virtual void Published(PublishContentContext context) { }
        public virtual Task PublishedAsync(PublishContentContext context)
        {
            Published(context);
            return Task.CompletedTask;
        }

        public virtual void Unpublishing(PublishContentContext context) { }
        public virtual Task UnpublishingAsync(PublishContentContext context)
        {
            Unpublishing(context);
            return Task.CompletedTask;
        }

        public virtual void Unpublished(PublishContentContext context) { }
        public virtual Task UnpublishedAsync(PublishContentContext context)
        {
            Unpublished(context);
            return Task.CompletedTask;
        }

        public virtual void Removing(RemoveContentContext context) { }
        public virtual Task RemovingAsync(RemoveContentContext context)
        {
            Removing(context);
            return Task.CompletedTask;
        }

        public virtual void Removed(RemoveContentContext context) { }
        public virtual Task RemovedAsync(RemoveContentContext context)
        {
            Removed(context);
            return Task.CompletedTask;
        }


        // TODO: Implement Clone event
        //protected virtual void Cloning(CloneContentContext context) { }
        //protected virtual void Cloned(CloneContentContext context) { }

        public virtual void GetContentItemAspect(ContentItemAspectContext context) { }
        public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            GetContentItemAspect(context);
            return Task.CompletedTask;
        }
    }
}