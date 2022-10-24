using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// An implementation of this class is called for all the fields of a content item.
    /// </summary>
    public interface IContentFieldHandler
    {
        Task ActivatedAsync(ActivatedContentContext context, ContentField field);
        Task ActivatingAsync(ActivatingContentContext context, ContentField field);
        Task InitializingAsync(InitializingContentContext context, ContentField field);
        Task InitializedAsync(InitializingContentContext context, ContentField field);
        Task CreatingAsync(CreateContentContext context, ContentField field);
        Task CreatedAsync(CreateContentContext context, ContentField field);
        Task LoadingAsync(LoadContentContext context, ContentField field);
        Task LoadedAsync(LoadContentContext context, ContentField field);
        Task ImportingAsync(ImportContentContext context, ContentField field);
        Task ImportedAsync(ImportContentContext context, ContentField field);
        Task UpdatingAsync(UpdateContentContext context, ContentField field);
        Task UpdatedAsync(UpdateContentContext context, ContentField field);
        Task ValidatingAsync(ValidateFieldContentContext context, ContentField field);
        Task ValidatedAsync(ValidateFieldContentContext context, ContentField field);
        Task VersioningAsync(VersionContentContext context, ContentField existing, ContentField building);
        Task VersionedAsync(VersionContentContext context, ContentField existing, ContentField building);
        Task DraftSavingAsync(SaveDraftContentContext context, ContentField field);
        Task DraftSavedAsync(SaveDraftContentContext context, ContentField field);
        Task PublishingAsync(PublishContentContext context, ContentField field);
        Task PublishedAsync(PublishContentContext context, ContentField field);
        Task UnpublishingAsync(PublishContentContext context, ContentField field);
        Task UnpublishedAsync(PublishContentContext context, ContentField field);
        Task RemovingAsync(RemoveContentContext context, ContentField field);
        Task RemovedAsync(RemoveContentContext context, ContentField field);
        Task GetContentItemAspectAsync(ContentItemAspectContext context, ContentField field);
        Task CloningAsync(CloneContentContext context, ContentField field);
        Task ClonedAsync(CloneContentContext context, ContentField field);
    }
}
