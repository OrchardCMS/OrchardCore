using Orchard.Events;

namespace Orchard.ContentTypes.Events {
    public interface IContentDefinitionEventHandler : IEventHandler {
        void ContentTypeCreated(ContentTypeCreatedContext context);
        void ContentTypeRemoved(ContentTypeRemovedContext context);
        void ContentTypeImporting(ContentTypeImportingContext context);
        void ContentTypeImported(ContentTypeImportedContext context);
        void ContentPartCreated(ContentPartCreatedContext context);
        void ContentPartRemoved(ContentPartRemovedContext context);
        void ContentPartAttached(ContentPartAttachedContext context);
        void ContentPartDetached(ContentPartDetachedContext context);
        void ContentPartImporting(ContentPartImportingContext context);
        void ContentPartImported(ContentPartImportedContext context);
        void ContentFieldAttached(ContentFieldAttachedContext context);
        void ContentFieldDetached(ContentFieldDetachedContext context);
    }
}