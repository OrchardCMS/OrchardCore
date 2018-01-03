using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    public abstract class ContentHandlerBase : IContentHandler
    {
        public virtual Task ActivatingAsync(ActivatingContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task ActivatedAsync(ActivatedContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task InitializingAsync(InitializingContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task InitializedAsync(InitializingContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task CreatingAsync(CreateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task CreatedAsync(CreateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task LoadingAsync(LoadContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task LoadedAsync(LoadContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatingAsync(UpdateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatedAsync(UpdateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task VersioningAsync(VersionContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task VersionedAsync(VersionContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task PublishingAsync(PublishContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task PublishedAsync(PublishContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UnpublishingAsync(PublishContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UnpublishedAsync(PublishContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task RemovingAsync(RemoveContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task RemovedAsync(RemoveContentContext context)
        {
            return Task.CompletedTask;
        }


        // TODO: Implement Clone event
        //protected virtual void Cloning(CloneContentContext context) { }
        //protected virtual void Cloned(CloneContentContext context) { }

        public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return Task.CompletedTask;
        }
    }
}