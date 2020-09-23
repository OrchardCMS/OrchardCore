using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// An implementation of this class is called for all the parts of a content item.
    /// </summary>
    public interface IContentPartHandler
    {
        Task ActivatedAsync(ActivatedContentContext context, ContentPart part);
        Task ActivatingAsync(ActivatingContentContext context, ContentPart part);
        Task InitializingAsync(InitializingContentContext context, ContentPart part);
        Task InitializedAsync(InitializingContentContext context, ContentPart part);
        Task CreatingAsync(CreateContentContext context, ContentPart part);
        Task CreatedAsync(CreateContentContext context, ContentPart part);
        Task LoadingAsync(LoadContentContext context, ContentPart part);
        Task LoadedAsync(LoadContentContext context, ContentPart part);
        Task ImportingAsync(ImportContentContext context, ContentPart part);
        Task ImportedAsync(ImportContentContext context, ContentPart part);
        Task UpdatingAsync(UpdateContentContext context, ContentPart part);
        Task UpdatedAsync(UpdateContentContext context, ContentPart part);
        Task ValidatingAsync(ValidateContentContext context, ContentPart part);
        Task ValidatedAsync(ValidateContentContext context, ContentPart part);
        Task VersioningAsync(VersionContentContext context, ContentPart existing, ContentPart building);
        Task VersionedAsync(VersionContentContext context, ContentPart existing, ContentPart building);
        Task DraftSavingAsync(SaveDraftContentContext context, ContentPart part);
        Task DraftSavedAsync(SaveDraftContentContext context, ContentPart part);
        Task PublishingAsync(PublishContentContext context, ContentPart part);
        Task PublishedAsync(PublishContentContext context, ContentPart part);
        Task UnpublishingAsync(PublishContentContext context, ContentPart part);
        Task UnpublishedAsync(PublishContentContext context, ContentPart part);
        Task RemovingAsync(RemoveContentContext context, ContentPart part);
        Task RemovedAsync(RemoveContentContext context, ContentPart part);
        Task GetContentItemAspectAsync(ContentItemAspectContext context, ContentPart part);
        Task CloningAsync(CloneContentContext context, ContentPart part);
        Task ClonedAsync(CloneContentContext context, ContentPart part);
    }
}
