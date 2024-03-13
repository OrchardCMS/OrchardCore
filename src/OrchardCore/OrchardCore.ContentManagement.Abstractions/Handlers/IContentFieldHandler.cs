using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// An implementation of this class is called for all the fields of a content item.
    /// </summary>
    public interface IContentFieldHandler
    {
        Task ActivatedAsync(ActivatedContentFieldContext context, ContentField field);
        Task ActivatingAsync(ActivatingContentFieldContext context, ContentField field);
        Task InitializingAsync(InitializingContentFieldContext context, ContentField field);
        Task InitializedAsync(InitializingContentFieldContext context, ContentField field);
        Task CreatingAsync(CreateContentFieldContext context, ContentField field);
        Task CreatedAsync(CreateContentFieldContext context, ContentField field);
        Task LoadingAsync(LoadContentFieldContext context, ContentField field);
        Task LoadedAsync(LoadContentFieldContext context, ContentField field);
        Task ImportingAsync(ImportContentFieldContext context, ContentField field);
        Task ImportedAsync(ImportContentFieldContext context, ContentField field);
        Task UpdatingAsync(UpdateContentFieldContext context, ContentField field);
        Task UpdatedAsync(UpdateContentFieldContext context, ContentField field);
        Task ValidatingAsync(ValidateContentFieldContext context, ContentField field);
        Task ValidatedAsync(ValidateContentFieldContext context, ContentField field);
        Task VersioningAsync(VersionContentFieldContext context, ContentField existing, ContentField building);
        Task VersionedAsync(VersionContentFieldContext context, ContentField existing, ContentField building);
        Task DraftSavingAsync(SaveDraftContentFieldContext context, ContentField field);
        Task DraftSavedAsync(SaveDraftContentFieldContext context, ContentField field);
        Task PublishingAsync(PublishContentFieldContext context, ContentField field);
        Task PublishedAsync(PublishContentFieldContext context, ContentField field);
        Task UnpublishingAsync(PublishContentFieldContext context, ContentField field);
        Task UnpublishedAsync(PublishContentFieldContext context, ContentField field);
        Task RemovingAsync(RemoveContentFieldContext context, ContentField field);
        Task RemovedAsync(RemoveContentFieldContext context, ContentField field);
        Task GetContentItemAspectAsync(ContentItemAspectContext context, ContentField field);
        Task CloningAsync(CloneContentFieldContext context, ContentField field);
        Task ClonedAsync(CloneContentFieldContext context, ContentField field);
    }
}
