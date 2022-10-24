using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;

public abstract class ContentFieldHandler<TField> : IContentFieldHandler where TField : ContentField, new()
{
    Task IContentFieldHandler.ActivatedAsync(ActivatedContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ActivatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ActivatingAsync(ActivatingContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ActivatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.InitializingAsync(InitializingContentContext context, ContentField field)
    {
        return field is TField tfield
            ? InitializingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.InitializedAsync(InitializingContentContext context, ContentField field)
    {
        return field is TField tfield
            ? InitializedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.CreatingAsync(CreateContentContext context, ContentField field)
    {
        return field is TField tfield
            ? CreatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.CreatedAsync(CreateContentContext context, ContentField field)
    {
        return field is TField tfield
            ? CreatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.LoadingAsync(LoadContentContext context, ContentField field)
    {
        return field is TField tfield
            ? LoadingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.LoadedAsync(LoadContentContext context, ContentField field)
    {
        return field is TField tfield
            ? LoadedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ImportingAsync(ImportContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ImportingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ImportedAsync(ImportContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ImportedAsync(context, tfield)
            : Task.CompletedTask;
    }
    Task IContentFieldHandler.UpdatingAsync(UpdateContentContext context, ContentField field)
    {
        return field is TField tfield
            ? UpdatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UpdatedAsync(UpdateContentContext context, ContentField field)
    {
        return field is TField tfield
            ? UpdatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ValidatingAsync(ValidateFieldContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ValidatingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.ValidatedAsync(ValidateFieldContentContext context, ContentField field)
    {
        return field is TField tfield
            ? ValidatedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.VersioningAsync(VersionContentContext context, ContentField existing, ContentField building)
    {
        return existing is TField texisting && building is TField tbuilding
            ? VersioningAsync(context, texisting, tbuilding)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.VersionedAsync(VersionContentContext context, ContentField existing, ContentField building)
    {
        return existing is TField texisting && building is TField tbuilding
            ? VersionedAsync(context, texisting, tbuilding)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.DraftSavingAsync(SaveDraftContentContext context, ContentField field)
    {
        return field is TField tfield
            ? DraftSavingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.DraftSavedAsync(SaveDraftContentContext context, ContentField field)
    {
        return field is TField tfield
            ? DraftSavedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.PublishingAsync(PublishContentContext context, ContentField field)
    {
        return field is TField tfield
            ? PublishingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.PublishedAsync(PublishContentContext context, ContentField field)
    {
        return field is TField tfield
            ? PublishedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UnpublishingAsync(PublishContentContext context, ContentField field)
    {
        return field is TField tfield
            ? UnpublishingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.UnpublishedAsync(PublishContentContext context, ContentField field)
    {
        return field is TField tfield
            ? UnpublishedAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.RemovingAsync(RemoveContentContext context, ContentField field)
    {
        return field is TField tfield
            ? RemovingAsync(context, tfield)
            : Task.CompletedTask;
    }

    Task IContentFieldHandler.RemovedAsync(RemoveContentContext context, ContentField field)
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
    async Task IContentFieldHandler.CloningAsync(CloneContentContext context, ContentField field)
    {
        if (field is TField tfield)
        {
            await CloningAsync(context, tfield);
        }
    }

    async Task IContentFieldHandler.ClonedAsync(CloneContentContext context, ContentField field)
    {
        if (field is TField tfield)
        {
            await ClonedAsync(context, tfield);
        }
    }

    public virtual Task ActivatedAsync(ActivatedContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task ActivatingAsync(ActivatingContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task InitializingAsync(InitializingContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task InitializedAsync(InitializingContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task CreatingAsync(CreateContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task CreatedAsync(CreateContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task LoadingAsync(LoadContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task LoadedAsync(LoadContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task ImportingAsync(ImportContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task ImportedAsync(ImportContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task UpdatingAsync(UpdateContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task UpdatedAsync(UpdateContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task ValidatingAsync(ValidateFieldContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task ValidatedAsync(ValidateFieldContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task VersioningAsync(VersionContentContext context, TField existing, TField building) => Task.CompletedTask;
    public virtual Task VersionedAsync(VersionContentContext context, TField existing, TField building) => Task.CompletedTask;
    public virtual Task DraftSavingAsync(SaveDraftContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task DraftSavedAsync(SaveDraftContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task PublishingAsync(PublishContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task PublishedAsync(PublishContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task UnpublishingAsync(PublishContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task UnpublishedAsync(PublishContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task RemovingAsync(RemoveContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task RemovedAsync(RemoveContentContext context, TField instance) => Task.CompletedTask;
    public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context, TField part) => Task.CompletedTask;
    public virtual Task CloningAsync(CloneContentContext context, TField part) => Task.CompletedTask;
    public virtual Task ClonedAsync(CloneContentContext context, TField part) => Task.CompletedTask;
}
