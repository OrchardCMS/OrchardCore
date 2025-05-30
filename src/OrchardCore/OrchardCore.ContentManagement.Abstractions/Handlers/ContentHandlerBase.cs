namespace OrchardCore.ContentManagement.Handlers;

public abstract class ContentHandlerBase : IContentHandler
{
    public virtual Task ActivatingAsync(ActivatingContentContext context)
        => Task.CompletedTask;

    public virtual Task ActivatedAsync(ActivatedContentContext context)
        => Task.CompletedTask;

    public virtual Task InitializingAsync(InitializingContentContext context)
        => Task.CompletedTask;

    public virtual Task InitializedAsync(InitializingContentContext context)
        => Task.CompletedTask;

    public virtual Task CreatingAsync(CreateContentContext context)
        => Task.CompletedTask;

    public virtual Task CreatedAsync(CreateContentContext context)
        => Task.CompletedTask;

    public virtual Task ImportingAsync(ImportContentContext context)
        => Task.CompletedTask;

    public virtual Task ImportedAsync(ImportContentContext context)
        => Task.CompletedTask;

    public virtual Task LoadingAsync(LoadContentContext context)
        => Task.CompletedTask;

    public virtual Task LoadedAsync(LoadContentContext context)
        => Task.CompletedTask;

    public virtual Task UpdatingAsync(UpdateContentContext context)
        => Task.CompletedTask;

    public virtual Task UpdatedAsync(UpdateContentContext context)
        => Task.CompletedTask;

    public virtual Task ValidatingAsync(ValidateContentContext context)
        => Task.CompletedTask;

    public virtual Task ValidatedAsync(ValidateContentContext context)
        => Task.CompletedTask;

    public virtual Task RestoringAsync(RestoreContentContext context)
        => Task.CompletedTask;

    public virtual Task RestoredAsync(RestoreContentContext context)
        => Task.CompletedTask;

    public virtual Task VersioningAsync(VersionContentContext context)
        => Task.CompletedTask;

    public virtual Task VersionedAsync(VersionContentContext context)
        => Task.CompletedTask;

    public virtual Task DraftSavingAsync(SaveDraftContentContext context)
        => Task.CompletedTask;

    public virtual Task DraftSavedAsync(SaveDraftContentContext context)
        => Task.CompletedTask;

    public virtual Task PublishingAsync(PublishContentContext context)
        => Task.CompletedTask;

    public virtual Task PublishedAsync(PublishContentContext context)
        => Task.CompletedTask;

    public virtual Task UnpublishingAsync(PublishContentContext context)
        => Task.CompletedTask;

    public virtual Task UnpublishedAsync(PublishContentContext context)
        => Task.CompletedTask;

    public virtual Task RemovingAsync(RemoveContentContext context)
        => Task.CompletedTask;

    public virtual Task RemovedAsync(RemoveContentContext context)
        => Task.CompletedTask;

    public virtual Task CloningAsync(CloneContentContext context)
        => Task.CompletedTask;

    public virtual Task ClonedAsync(CloneContentContext context)
        => Task.CompletedTask;

    public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context)
        => Task.CompletedTask;

    public virtual Task ImportCompletedAsync(ImportedContentsContext context)
        => Task.CompletedTask;
}
