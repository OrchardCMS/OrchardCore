using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.ContentManagement.Handlers {
    public abstract class ContentHandler : IContentHandler {
        protected ContentHandler() {
            Filters = new List<IContentFilter>();
        }

        public List<IContentFilter> Filters { get; set; }

        protected void OnActivated<TPart>(Action<ActivatedContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnActivated = handler });
        }

        protected void OnInitializing<TPart>(Action<InitializingContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnInitializing = handler });
        }

        protected void OnInitialized<TPart>(Action<InitializingContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnInitialized = handler });
        }

        protected void OnCreating<TPart>(Action<CreateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreating = handler });
        }

        protected void OnCreated<TPart>(Action<CreateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnCreated = handler });
        }

        protected void OnLoading<TPart>(Action<LoadContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoading = handler });
        }

        protected void OnLoaded<TPart>(Action<LoadContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnLoaded = handler });
        }

        protected void OnUpdating<TPart>(Action<UpdateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnUpdating = handler });
        }

        protected void OnUpdated<TPart>(Action<UpdateContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnUpdated = handler });
        }

        protected void OnVersioning<TPart>(Action<VersionContentContext, TPart, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnVersioning = handler });
        }

        protected void OnVersioned<TPart>(Action<VersionContentContext, TPart, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnVersioned = handler });
        }

        protected void OnPublishing<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnPublishing = handler });
        }

        protected void OnPublished<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnPublished = handler });
        }

        protected void OnUnpublishing<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnUnpublishing = handler });
        }

        protected void OnUnpublished<TPart>(Action<PublishContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnUnpublished = handler });
        }

        protected void OnRemoving<TPart>(Action<RemoveContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnRemoving = handler });
        }

        protected void OnRemoved<TPart>(Action<RemoveContentContext, TPart> handler) where TPart : class, IContent {
            Filters.Add(new InlineStorageFilter<TPart> { OnRemoved = handler });
        }

        //protected void OnDestroying<TPart>(Action<DestroyContentContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineStorageFilter<TPart> { OnDestroying = handler });
        //}

        //protected void OnDestroyed<TPart>(Action<DestroyContentContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineStorageFilter<TPart> { OnDestroyed = handler });
        //}

        //protected void OnIndexing<TPart>(Action<IndexContentContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineStorageFilter<TPart> { OnIndexing = handler });
        //}

        //protected void OnIndexed<TPart>(Action<IndexContentContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineStorageFilter<TPart> { OnIndexed = handler });
        //}

        //protected void OnGetContentItemMetadata<TPart>(Action<GetContentItemMetadataContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineTemplateFilter<TPart> { OnGetItemMetadata = handler });
        //}
        //protected void OnGetDisplayShape<TPart>(Action<BuildDisplayContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineTemplateFilter<TPart> { OnGetDisplayShape = handler });
        //}

        //protected void OnGetEditorShape<TPart>(Action<BuildEditorContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineTemplateFilter<TPart> { OnGetEditorShape = handler });
        //}

        //protected void OnUpdateEditorShape<TPart>(Action<UpdateEditorContext, TPart> handler) where TPart : class, IContent {
        //    Filters.Add(new InlineTemplateFilter<TPart> { OnUpdateEditorShape = handler });
        //}

        class InlineStorageFilter<TPart> : StorageFilterBase<TPart> where TPart : class, IContent {
            public Action<ActivatedContentContext, TPart> OnActivated { get; set; }
            public Action<InitializingContentContext, TPart> OnInitializing { get; set; }
            public Action<InitializingContentContext, TPart> OnInitialized { get; set; }
            public Action<CreateContentContext, TPart> OnCreating { get; set; }
            public Action<CreateContentContext, TPart> OnCreated { get; set; }
            public Action<LoadContentContext, TPart> OnLoading { get; set; }
            public Action<LoadContentContext, TPart> OnLoaded { get; set; }
            public Action<UpdateContentContext, TPart> OnUpdating { get; set; }
            public Action<UpdateContentContext, TPart> OnUpdated { get; set; }
            public Action<VersionContentContext, TPart, TPart> OnVersioning { get; set; }
            public Action<VersionContentContext, TPart, TPart> OnVersioned { get; set; }
            public Action<PublishContentContext, TPart> OnPublishing { get; set; }
            public Action<PublishContentContext, TPart> OnPublished { get; set; }
            public Action<PublishContentContext, TPart> OnUnpublishing { get; set; }
            public Action<PublishContentContext, TPart> OnUnpublished { get; set; }
            public Action<RemoveContentContext, TPart> OnRemoving { get; set; }
            public Action<RemoveContentContext, TPart> OnRemoved { get; set; }
            //public Action<IndexContentContext, TPart> OnIndexing { get; set; }
            //public Action<IndexContentContext, TPart> OnIndexed { get; set; }
            //public Action<RestoreContentContext, TPart> OnRestoring { get; set; }
            //public Action<RestoreContentContext, TPart> OnRestored { get; set; }
            //public Action<DestroyContentContext, TPart> OnDestroying { get; set; }
            //public Action<DestroyContentContext, TPart> OnDestroyed { get; set; }
            protected override void Activated(ActivatedContentContext context, TPart instance) {
                if (OnActivated != null) OnActivated(context, instance);
            }
            protected override void Initializing(InitializingContentContext context, TPart instance) {
                if (OnInitializing != null) OnInitializing(context, instance);
            }
            protected override void Initialized(InitializingContentContext context, TPart instance) {
                if (OnInitialized != null) OnInitialized(context, instance);
            }
            protected override void Creating(CreateContentContext context, TPart instance) {
                if (OnCreating != null) OnCreating(context, instance);
            }
            protected override void Created(CreateContentContext context, TPart instance) {
                if (OnCreated != null) OnCreated(context, instance);
            }
            protected override void Loading(LoadContentContext context, TPart instance) {
                if (OnLoading != null) OnLoading(context, instance);
            }
            protected override void Loaded(LoadContentContext context, TPart instance) {
                if (OnLoaded != null) OnLoaded(context, instance);
            }
            protected override void Updating(UpdateContentContext context, TPart instance) {
                if (OnUpdating != null) OnUpdating(context, instance);
            }
            protected override void Updated(UpdateContentContext context, TPart instance) {
                if (OnUpdated != null) OnUpdated(context, instance);
            }
            protected override void Versioning(VersionContentContext context, TPart existing, TPart building) {
                if (OnVersioning != null) OnVersioning(context, existing, building);
            }
            protected override void Versioned(VersionContentContext context, TPart existing, TPart building) {
                if (OnVersioned != null) OnVersioned(context, existing, building);
            }
            protected override void Publishing(PublishContentContext context, TPart instance) {
                if (OnPublishing != null) OnPublishing(context, instance);
            }
            protected override void Published(PublishContentContext context, TPart instance) {
                if (OnPublished != null) OnPublished(context, instance);
            }
            protected override void Unpublishing(PublishContentContext context, TPart instance) {
                if (OnUnpublishing != null) OnUnpublishing(context, instance);
            }
            protected override void Unpublished(PublishContentContext context, TPart instance) {
                if (OnUnpublished != null) OnUnpublished(context, instance);
            }
            protected override void Removing(RemoveContentContext context, TPart instance) {
                if (OnRemoving != null) OnRemoving(context, instance);
            }
            protected override void Removed(RemoveContentContext context, TPart instance) {
                if (OnRemoved != null) OnRemoved(context, instance);
            }
            //protected override void Indexing(IndexContentContext context, TPart instance) {
            //    if ( OnIndexing != null )
            //        OnIndexing(context, instance);
            //}
            //protected override void Indexed(IndexContentContext context, TPart instance) {
            //    if ( OnIndexed != null )
            //        OnIndexed(context, instance);
            //}
            //protected override void Restoring(RestoreContentContext context, TPart instance) {
            //    if (OnRestoring != null)
            //        OnRestoring(context, instance);
            //}
            //protected override void Restored(RestoreContentContext context, TPart instance) {
            //    if (OnRestored != null)
            //        OnRestored(context, instance);
            //}
            //protected override void Destroying(DestroyContentContext context, TPart instance) {
            //    if (OnDestroying != null)
            //        OnDestroying(context, instance);
            //}
            //protected override void Destroyed(DestroyContentContext context, TPart instance) {
            //    if (OnDestroyed != null)
            //        OnDestroyed(context, instance);
            //}
        }

        void IContentHandler.Activating(ActivatingContentContext context) {
            foreach (var filter in Filters.OfType<IContentActivatingFilter>())
                filter.Activating(context);
            Activating(context);
        }

        void IContentHandler.Activated(ActivatedContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Activated(context);
            Activated(context);
        }

        void IContentHandler.Initializing(InitializingContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Initializing(context);
            Initializing(context);
        }

        void IContentHandler.Initialized(InitializingContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Initialized(context);
            Initialized(context);
        }
        
        void IContentHandler.Creating(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Creating(context);
            Creating(context);
        }

        void IContentHandler.Created(CreateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Created(context);
            Created(context);
        }

        void IContentHandler.Loading(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loading(context);
            Loading(context);
        }

        void IContentHandler.Loaded(LoadContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Loaded(context);
            Loaded(context);
        }

        void IContentHandler.Updating(UpdateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Updating(context);
            Updating(context);
        }

        void IContentHandler.Updated(UpdateContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Updated(context);
            Updated(context);
        }

        void IContentHandler.Versioning(VersionContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Versioning(context);
            Versioning(context);
        }

        void IContentHandler.Versioned(VersionContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Versioned(context);
            Versioned(context);
        }

        void IContentHandler.Publishing(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Publishing(context);
            Publishing(context);
        }

        void IContentHandler.Published(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Published(context);
            Published(context);
        }

        void IContentHandler.Unpublishing(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Unpublishing(context);
            Unpublishing(context);
        }

        void IContentHandler.Unpublished(PublishContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Unpublished(context);
            Unpublished(context);
        }

        void IContentHandler.Removing(RemoveContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Removing(context);
            Removing(context);
        }

        void IContentHandler.Removed(RemoveContentContext context) {
            foreach (var filter in Filters.OfType<IContentStorageFilter>())
                filter.Removed(context);
            Removed(context);
        }

        //void IContentHandler.Indexing(IndexContentContext context) {
        //    foreach ( var filter in Filters.OfType<IContentStorageFilter>() )
        //        filter.Indexing(context);
        //    Indexing(context);
        //}

        //void IContentHandler.Indexed(IndexContentContext context) {
        //    foreach ( var filter in Filters.OfType<IContentStorageFilter>() )
        //        filter.Indexed(context);
        //    Indexed(context);
        //}

        //void IContentHandler.Importing(ImportContentContext context) {
        //    Importing(context);
        //}

        //void IContentHandler.Imported(ImportContentContext context) {
        //    Imported(context);
        //}

        //void IContentHandler.Exporting(ExportContentContext context) {
        //    Exporting(context);
        //}

        //void IContentHandler.Exported(ExportContentContext context) {
        //    Exported(context);
        //}

        //void IContentHandler.Restoring(RestoreContentContext context) {
        //    foreach (var filter in Filters.OfType<IContentStorageFilter>())
        //        filter.Restoring(context);
        //    Restoring(context);
        //}

        //void IContentHandler.Restored(RestoreContentContext context) {
        //    foreach (var filter in Filters.OfType<IContentStorageFilter>())
        //        filter.Restored(context);
        //    Restored(context);
        //}

        //void IContentHandler.Destroying(DestroyContentContext context) {
        //    foreach (var filter in Filters.OfType<IContentStorageFilter>())
        //        filter.Destroying(context);
        //    Destroying(context);
        //}

        //void IContentHandler.Destroyed(DestroyContentContext context) {
        //    foreach (var filter in Filters.OfType<IContentStorageFilter>())
        //        filter.Destroyed(context);
        //    Destroyed(context);
        //}

        //void IContentHandler.GetContentItemMetadata(GetContentItemMetadataContext context) {
        //    foreach (var filter in Filters.OfType<IContentTemplateFilter>())
        //        filter.GetContentItemMetadata(context);
        //    GetItemMetadata(context);
        //}
        //void IContentHandler.BuildDisplay(BuildDisplayContext context) {
        //    foreach (var filter in Filters.OfType<IContentTemplateFilter>())
        //        filter.BuildDisplayShape(context);
        //    BuildDisplayShape(context);
        //}
        //void IContentHandler.BuildEditor(BuildEditorContext context) {
        //    foreach (var filter in Filters.OfType<IContentTemplateFilter>())
        //        filter.BuildEditorShape(context);
        //    BuildEditorShape(context);
        //}
        //void IContentHandler.UpdateEditor(UpdateEditorContext context) {
        //    foreach (var filter in Filters.OfType<IContentTemplateFilter>())
        //        filter.UpdateEditorShape(context);
        //    UpdateEditorShape(context);
        //}

        protected virtual void Activating(ActivatingContentContext context) { }
        protected virtual void Activated(ActivatedContentContext context) { }

        protected virtual void Initializing(InitializingContentContext context) { }
        protected virtual void Initialized(InitializingContentContext context) { }

        protected virtual void Creating(CreateContentContext context) { }
        protected virtual void Created(CreateContentContext context) { }

        protected virtual void Loading(LoadContentContext context) { }
        protected virtual void Loaded(LoadContentContext context) { }

        protected virtual void Updating(UpdateContentContext context) { }
        protected virtual void Updated(UpdateContentContext context) { }

        protected virtual void Versioning(VersionContentContext context) { }
        protected virtual void Versioned(VersionContentContext context) { }

        protected virtual void Publishing(PublishContentContext context) { }
        protected virtual void Published(PublishContentContext context) { }

        protected virtual void Unpublishing(PublishContentContext context) { }
        protected virtual void Unpublished(PublishContentContext context) { }

        protected virtual void Removing(RemoveContentContext context) { }
        protected virtual void Removed(RemoveContentContext context) { }

        //protected virtual void Indexing(IndexContentContext context) { }
        //protected virtual void Indexed(IndexContentContext context) { }

        //protected virtual void Importing(ImportContentContext context) { }
        //protected virtual void Imported(ImportContentContext context) { }
        //protected virtual void Exporting(ExportContentContext context) { }
        //protected virtual void Exported(ExportContentContext context) { }
        //protected virtual void Restoring(RestoreContentContext context) { }
        //protected virtual void Restored(RestoreContentContext context) { }
        //protected virtual void Destroying(DestroyContentContext context) { }
        //protected virtual void Destroyed(DestroyContentContext context) { }
    }
}