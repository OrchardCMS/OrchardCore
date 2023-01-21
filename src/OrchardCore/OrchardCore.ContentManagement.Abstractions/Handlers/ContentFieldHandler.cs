using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;

public abstract class ContentFieldHandler<TField> : IContentFieldHandler where TField : ContentField, new()
{
    Task IContentFieldHandler.ActivatedAsync(ActivatedContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ActivatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ActivatingAsync(ActivatingContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ActivatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.InitializingAsync(InitializingContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? InitializingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.InitializedAsync(InitializingContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? InitializedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.CreatingAsync(CreateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? CreatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.CreatedAsync(CreateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? CreatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.LoadingAsync(LoadContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? LoadingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.LoadedAsync(LoadContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? LoadedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ImportingAsync(ImportContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ImportingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ImportedAsync(ImportContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ImportedAsync(context, tfield)
            : Task.CompletedTask;
    }
    Task IContentFieldHandler.UpdatingAsync(UpdateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? UpdatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UpdatedAsync(UpdateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? UpdatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ValidatingAsync(ValidateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ValidatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ValidatedAsync(ValidateContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? ValidatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.VersioningAsync(VersionContentFieldContext context, ContentField existing, ContentField building)
    {
        return existing is TField texisting && building is TField tbuilding
            ? VersioningAsync(context, texisting, tbuilding)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.VersionedAsync(VersionContentFieldContext context, ContentField existing, ContentField building)
    {
        return existing is TField texisting && building is TField tbuilding
            ? VersionedAsync(context, texisting, tbuilding)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.DraftSavingAsync(SaveDraftContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? DraftSavingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.DraftSavedAsync(SaveDraftContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? DraftSavedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.PublishingAsync(PublishContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? PublishingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.PublishedAsync(PublishContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? PublishedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UnpublishingAsync(PublishContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? UnpublishingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UnpublishedAsync(PublishContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? UnpublishedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.RemovingAsync(RemoveContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? RemovingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.RemovedAsync(RemoveContentFieldContext context, ContentField field)
    {
        return field is TField tfield
            ? RemovedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.GetContentItemAspectAsync(ContentItemAspectContext context, ContentField field)
    {
        return field is TField tfield
            ? GetContentItemAspectAsync(context, tfield)
            : Task.CompletedTask;
    }
    async Task IContentFieldHandler.CloningAsync(CloneContentFieldContext context, ContentField field)
    {
        if (field is TField tfield)
        {
            await CloningAsync(context, tfield);
        }
    }

    async Task IContentFieldHandler.ClonedAsync(CloneContentFieldContext context, ContentField field)
    {
        if (field is TField tfield)
        {
            await ClonedAsync(context, tfield);
        }
    }

    public virtual Task ActivatedAsync(ActivatedContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ActivatingAsync(ActivatingContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task InitializingAsync(InitializingContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task InitializedAsync(InitializingContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task CreatingAsync(CreateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task CreatedAsync(CreateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task LoadingAsync(LoadContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task LoadedAsync(LoadContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ImportingAsync(ImportContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ImportedAsync(ImportContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task UpdatingAsync(UpdateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task UpdatedAsync(UpdateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ValidatingAsync(ValidateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ValidatedAsync(ValidateContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task VersioningAsync(VersionContentFieldContext context, TField existing, TField building) => Task.CompletedTask;
    public virtual Task VersionedAsync(VersionContentFieldContext context, TField existing, TField building) => Task.CompletedTask;
    public virtual Task DraftSavingAsync(SaveDraftContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task DraftSavedAsync(SaveDraftContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task PublishingAsync(PublishContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task PublishedAsync(PublishContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task UnpublishingAsync(PublishContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task UnpublishedAsync(PublishContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task RemovingAsync(RemoveContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task RemovedAsync(RemoveContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context, TField field) => Task.CompletedTask;
    public virtual Task CloningAsync(CloneContentFieldContext context, TField field) => Task.CompletedTask;
    public virtual Task ClonedAsync(CloneContentFieldContext context, TField field) => Task.CompletedTask;
}
