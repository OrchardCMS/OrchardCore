using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    public interface IContentHandler
    {
        Task ActivatingAsync(ActivatingContentContext context);
        Task ActivatedAsync(ActivatedContentContext context);
        Task InitializingAsync(InitializingContentContext context);
        Task InitializedAsync(InitializingContentContext context);
        Task CreatingAsync(CreateContentContext context);
        Task CreatedAsync(CreateContentContext context);
        Task LoadingAsync(LoadContentContext context);
        Task LoadedAsync(LoadContentContext context);
        Task ImportingAsync(ImportContentContext context);
        Task ImportedAsync(ImportContentContext context);
        Task UpdatingAsync(UpdateContentContext context);
        Task UpdatedAsync(UpdateContentContext context);
        Task ValidatingAsync(ValidateContentContext context);
        Task ValidatedAsync(ValidateContentContext context);
        Task RestoringAsync(RestoreContentContext context);
        Task RestoredAsync(RestoreContentContext context);
        Task VersioningAsync(VersionContentContext context);
        Task VersionedAsync(VersionContentContext context);
        Task DraftSavingAsync(SaveDraftContentContext context);
        Task DraftSavedAsync(SaveDraftContentContext context);
        Task PublishingAsync(PublishContentContext context);
        Task PublishedAsync(PublishContentContext context);
        Task UnpublishingAsync(PublishContentContext context);
        Task UnpublishedAsync(PublishContentContext context);
        Task RemovingAsync(RemoveContentContext context);
        Task RemovedAsync(RemoveContentContext context);
        Task GetContentItemAspectAsync(ContentItemAspectContext context);
        Task CloningAsync(CloneContentContext context);
        Task ClonedAsync(CloneContentContext context);
    }
}
