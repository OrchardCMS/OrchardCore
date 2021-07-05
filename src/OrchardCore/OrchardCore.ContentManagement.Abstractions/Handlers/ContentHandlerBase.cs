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

        public virtual Task ImportingAsync(ImportContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task ImportedAsync(ImportContentContext context)
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

        public virtual Task ValidatingAsync(ValidateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task ValidatedAsync(ValidateContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task RestoringAsync(RestoreContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task RestoredAsync(RestoreContentContext context)
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

        public virtual Task DraftSavingAsync(SaveDraftContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task DraftSavedAsync(SaveDraftContentContext context)
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

        public virtual Task CloningAsync(CloneContentContext context)
        {
            return Task.CompletedTask;
        }
        public virtual Task ClonedAsync(CloneContentContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return Task.CompletedTask;
        }
    }
}
