using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public abstract class ContentPartDriver<TPart> : IContentHandler, IContentPartDriver where TPart : ContentPart, new()
    {
        private static readonly ContentPartInfo _contentPartInfo;
        static ContentPartDriver()
        {
            _contentPartInfo = new ContentPartInfo
            {
                PartName = typeof(TPart).Name,
                Factory = typePartDefinition => new TPart()
            };
        }

        void IContentHandler.Activating(ActivatingContentContext context)
        {
            // Activating is not available for Content Parts
        }

        void IContentHandler.Activated(ActivatedContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Activated(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Initializing(InitializingContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Initializing(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Initialized(InitializingContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Initialized(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Creating(CreateContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Creating(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Created(CreateContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Created(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Loading(LoadContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Loading(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Loaded(LoadContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Loaded(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Updating(UpdateContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Updating(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Updated(UpdateContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Updated(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Versioning(VersionContentContext context)
        {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioning(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentHandler.Versioned(VersionContentContext context)
        {
            if (context.ExistingContentItem.Is<TPart>() || context.BuildingContentItem.Is<TPart>())
                Versioned(context, context.ExistingContentItem.As<TPart>(), context.BuildingContentItem.As<TPart>());
        }

        void IContentHandler.Publishing(PublishContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Publishing(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Published(PublishContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Published(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Unpublishing(PublishContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Unpublishing(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Unpublished(PublishContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Unpublished(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Removing(RemoveContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Removing(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.Removed(RemoveContentContext context)
        {
            if (context.ContentItem.Is<TPart>())
                Removed(context, context.ContentItem.As<TPart>());
        }

        void IContentHandler.GetContentItemMetadata(ContentItemMetadataContext context)
        {
            if (context.ContentItem.Is<TPart>())
                GetContentItemMetadata(context, context.ContentItem.As<TPart>());
        }

        protected virtual void Activated(ActivatedContentContext context, TPart instance) { }
        protected virtual void Activating(ActivatingContentContext context, TPart instance) { }
        protected virtual void Initializing(InitializingContentContext context, TPart instance) { }
        protected virtual void Initialized(InitializingContentContext context, TPart instance) { }
        protected virtual void Creating(CreateContentContext context, TPart instance) { }
        protected virtual void Created(CreateContentContext context, TPart instance) { }
        protected virtual void Loading(LoadContentContext context, TPart instance) { }
        protected virtual void Loaded(LoadContentContext context, TPart instance) { }
        protected virtual void Updating(UpdateContentContext context, TPart instance) { }
        protected virtual void Updated(UpdateContentContext context, TPart instance) { }
        protected virtual void Versioning(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Versioned(VersionContentContext context, TPart existing, TPart building) { }
        protected virtual void Publishing(PublishContentContext context, TPart instance) { }
        protected virtual void Published(PublishContentContext context, TPart instance) { }
        protected virtual void Unpublishing(PublishContentContext context, TPart instance) { }
        protected virtual void Unpublished(PublishContentContext context, TPart instance) { }
        protected virtual void Removing(RemoveContentContext context, TPart instance) { }
        protected virtual void Removed(RemoveContentContext context, TPart instance) { }
        protected virtual void GetContentItemMetadata(ContentItemMetadataContext context, TPart part) { }

        ContentPartInfo IContentPartDriver.GetPartInfo()
        {
            return _contentPartInfo;
        }

        //protected virtual void Indexing(IndexContentContext context, TPart instance) { }
        //protected virtual void Indexed(IndexContentContext context, TPart instance) { }
        //protected virtual void Restoring(RestoreContentContext context, TPart instance) { }
        //protected virtual void Restored(RestoreContentContext context, TPart instance) { }
        //protected virtual void Destroying(DestroyContentContext context, TPart instance) { }
        //protected virtual void Destroyed(DestroyContentContext context, TPart instance) { }


    }
}