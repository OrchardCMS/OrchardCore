namespace OrchardCore.ContentTypes.Events;

public interface IContentDefinitionEventHandler
{
    void ContentTypeCreated(ContentTypeCreatedContext context);

    void ContentTypeUpdated(ContentTypeUpdatedContext context);

    void ContentTypeRemoved(ContentTypeRemovedContext context);

    void ContentTypeImporting(ContentTypeImportingContext context);

    void ContentTypeImported(ContentTypeImportedContext context);

    void ContentPartCreated(ContentPartCreatedContext context);

    void ContentPartUpdated(ContentPartUpdatedContext context);

    void ContentPartRemoved(ContentPartRemovedContext context);

    void ContentPartAttached(ContentPartAttachedContext context);

    void ContentPartDetached(ContentPartDetachedContext context);

    void ContentPartImporting(ContentPartImportingContext context);

    void ContentPartImported(ContentPartImportedContext context);

    void ContentTypePartUpdated(ContentTypePartUpdatedContext context);

    void ContentFieldAttached(ContentFieldAttachedContext context);

    void ContentFieldUpdated(ContentFieldUpdatedContext context);

    void ContentFieldDetached(ContentFieldDetachedContext context);

    void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context);
}
