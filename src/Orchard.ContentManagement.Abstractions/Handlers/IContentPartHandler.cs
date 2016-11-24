namespace Orchard.ContentManagement.Handlers
{
    /// <summary>
    /// An implementation of this class is called for all the parts of a content item.
    /// </summary>
    public interface IContentPartHandler
    {
        void Activated(ActivatedContentContext context, ContentPart part);
        void Activating(ActivatingContentContext context, ContentPart part);
        void Initializing(InitializingContentContext context, ContentPart part);
        void Initialized(InitializingContentContext context, ContentPart part);
        void Creating(CreateContentContext context, ContentPart part);
        void Created(CreateContentContext context, ContentPart part);
        void Loading(LoadContentContext context, ContentPart part);
        void Loaded(LoadContentContext context, ContentPart part);
        void Updating(UpdateContentContext context, ContentPart part);
        void Updated(UpdateContentContext context, ContentPart part);
        void Versioning(VersionContentContext context, ContentPart existing, ContentPart building);
        void Versioned(VersionContentContext context, ContentPart existing, ContentPart building);
        void Publishing(PublishContentContext context, ContentPart part);
        void Published(PublishContentContext context, ContentPart part);
        void Unpublishing(PublishContentContext context, ContentPart part);
        void Unpublished(PublishContentContext context, ContentPart part);
        void Removing(RemoveContentContext context, ContentPart part);
        void Removed(RemoveContentContext context, ContentPart part);
        void GetContentItemAspect(ContentItemAspectContext context, ContentPart part);
    }
}
