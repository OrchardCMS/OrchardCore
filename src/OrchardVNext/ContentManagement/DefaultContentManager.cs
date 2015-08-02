using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.Data;
using OrchardVNext.ContentManagement.Handlers;
using OrchardVNext.ContentManagement.MetaData;
using OrchardVNext.ContentManagement.MetaData.Builders;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement {
    public class DefaultContentManager : IContentManager {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IEnumerable<IContentHandler> _handlers;
        private readonly IContentItemStore _contentItemStore;
        private readonly IContentStorageManager _contentStorageManager;

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IContentManagerSession contentManagerSession,
            IEnumerable<IContentHandler> handlers,
            IContentItemStore contentItemStore,
            IContentStorageManager contentStorageManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManagerSession = contentManagerSession;
            _handlers = handlers;
            _contentItemStore = contentItemStore;
            _contentStorageManager = contentStorageManager;
        }

        public IEnumerable<IContentHandler> Handlers => _handlers;

        public IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions() {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public virtual ContentItem New(string contentType) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null) {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

            // invoke handlers to weld aspects onto kernel
            Handlers.Invoke(handler => handler.Activating(context));

            var context2 = new ActivatedContentContext {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            // back-reference for convenience (e.g. getting metadata when in a view)
            context2.ContentItem.ContentManager = this;

            Handlers.Invoke(handler => handler.Activated(context2));

            var context3 = new InitializingContentContext {
                ContentType = context2.ContentType,
                ContentItem = context2.ContentItem,
            };

            Handlers.Invoke(handler => handler.Initializing(context3));
            Handlers.Invoke(handler => handler.Initialized(context3));

            // composite result is returned
            return context3.ContentItem;
        }

        public virtual ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public virtual ContentItem Get(int id, VersionOptions options) {
            ContentItem contentItem;

            ContentItemVersionRecord versionRecord = null;

            // obtain the root records based on version options
            if (options.VersionRecordId != 0) {
                // short-circuit if item held in session
                if (_contentManagerSession.RecallVersionRecordId(options.VersionRecordId, out contentItem)) {
                    return contentItem;
                }

                versionRecord = _contentItemStore.Get(id, options).VersionRecord;
            }
            else if (options.VersionNumber != 0) {
                // short-circuit if item held in session
                if (_contentManagerSession.RecallVersionNumber(id, options.VersionNumber, out contentItem)) {
                    return contentItem;
                }

                versionRecord = _contentItemStore.Get(id, options).VersionRecord;
            }
            else if (_contentManagerSession.RecallContentRecordId(id, out contentItem)) {
                // try to reload a previously loaded published content item

                if (options.IsPublished) {
                    return contentItem;
                }

                versionRecord = contentItem.VersionRecord;
            }

            // no record means content item is not in db
            if (versionRecord == null) {
                // check in memory
                var record = _contentItemStore.Get(id, options).VersionRecord;
                if (record == null) {
                    return null;
                }

                versionRecord = record;
            }

            // return item if obtained earlier in session
            if (_contentManagerSession.RecallVersionRecordId(versionRecord.Id, out contentItem)) {
                if (options.IsDraftRequired && versionRecord.Published) {
                    return BuildNewVersion(contentItem);
                }
                return contentItem;
            }

            // allocate instance and set record property
            contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
            contentItem.VersionRecord = versionRecord;

            // store in session prior to loading to avoid some problems with simple circular dependencies
            _contentManagerSession.Store(contentItem);

            // create a context with a new instance to load            
            var context = new LoadContentContext(contentItem);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Loading(context));
            Handlers.Invoke(handler => handler.Loaded(context));

            // when draft is required and latest is published a new version is appended 
            if (options.IsDraftRequired && versionRecord.Published) {
                contentItem = BuildNewVersion(context.ContentItem);
            }

            return contentItem;
        }
        
        public virtual void Publish(ContentItem contentItem) {
            if (contentItem.VersionRecord.Published) {
                return;
            }
            // create a context for the item and it's previous published record
            var previous = contentItem.Record.Versions.SingleOrDefault(x => x.Published);
            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Publishing(context));

            if (context.Cancel) {
                return;
            }

            if (previous != null) {
                previous.Published = false;
            }
            contentItem.VersionRecord.Published = true;

            Handlers.Invoke(handler => handler.Published(context));
        }

        public virtual void Unpublish(ContentItem contentItem) {
            ContentItem publishedItem;
            if (contentItem.VersionRecord.Published) {
                // the version passed in is the published one
                publishedItem = contentItem;
            }
            else {
                // try to locate the published version of this item
                publishedItem = Get(contentItem.Id, VersionOptions.Published);
            }

            if (publishedItem == null) {
                // no published version exists. no work to perform.
                return;
            }

            // create a context for the item. the publishing version is null in this case
            // and the previous version is the one active prior to unpublishing. handlers
            // should take this null check into account
            var context = new PublishContentContext(contentItem, publishedItem.VersionRecord) {
                PublishingItemVersionRecord = null
            };

            Handlers.Invoke(handler => handler.Unpublishing(context));

            publishedItem.VersionRecord.Published = false;

            Handlers.Invoke(handler => handler.Unpublished(context));
        }
        
        protected virtual ContentItem BuildNewVersion(ContentItem existingContentItem) {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord {
                ContentItemRecord = contentItemRecord,
                Latest = true,
                Published = false,
                Data = existingItemVersionRecord.Data,
            };


            var latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

            if (latestVersion != null) {
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else {
                buildingItemVersionRecord.Number = contentItemRecord.Versions.Max(x => x.Number) + 1;
            }

            contentItemRecord.Versions.Add(buildingItemVersionRecord);
            _contentStorageManager.Store(buildingItemVersionRecord);

            var buildingContentItem = New(existingContentItem.ContentType);
            buildingContentItem.VersionRecord = buildingItemVersionRecord;

            var context = new VersionContentContext {
                Id = existingContentItem.Id,
                ContentType = existingContentItem.ContentType,
                ContentItemRecord = contentItemRecord,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
                ExistingItemVersionRecord = existingItemVersionRecord,
                BuildingItemVersionRecord = buildingItemVersionRecord,
            };
            Handlers.Invoke(handler => handler.Versioning(context));
            Handlers.Invoke(handler => handler.Versioned(context));

            return context.BuildingContentItem;
        }

        public virtual void Create(ContentItem contentItem) {
            Create(contentItem, VersionOptions.Published);
        }

        public virtual void Create(ContentItem contentItem, VersionOptions options) {
            if (contentItem.VersionRecord == null) {
                // produce root record to determine the model id
                contentItem.VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord(),
                    Number = 1,
                    Latest = true,
                    Published = true
                };
            }

            // add to the collection manually for the created case
            contentItem.VersionRecord.ContentItemRecord.Versions.Add(contentItem.VersionRecord);
            contentItem.VersionRecord.ContentItemRecord.ContentType = AcquireContentTypeRecord(contentItem.ContentType);

            // version may be specified
            if (options.VersionNumber != 0) {
                contentItem.VersionRecord.Number = options.VersionNumber;
            }

            // draft flag on create is required for explicitly-published content items
            if (options.IsDraft) {
                contentItem.VersionRecord.Published = false;
            }

            _contentItemStore.Store(contentItem);

            // build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);

            // invoke handlers to add information to persistent stores
            Handlers.Invoke(handler => handler.Creating(context));

            Handlers.Invoke(handler => handler.Created(context));

            if (options.IsPublished) {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Publishing(publishContext));

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Published(publishContext));
            }
        }

        private ContentTypeRecord AcquireContentTypeRecord(string contentType) {
            
            var contentTypeRecord = _contentStorageManager
                .Query<ContentTypeRecord>(x => x.Name == contentType)
                .SingleOrDefault();

            if (contentTypeRecord == null) {
                //TEMP: this is not safe... ContentItem types could be created concurrently?
                contentTypeRecord = new ContentTypeRecord { Name = contentType };
                _contentStorageManager.Store(contentTypeRecord);
            }

            var contentTypeId = contentTypeRecord.Id;

            // There is a case when a content type record is created locally but the transaction is actually
            // cancelled. In this case we are caching an Id which is none existent, or might represent another
            // content type. Thus we need to ensure that the cache is valid, or invalidate it and retrieve it 
            // another time.

            var result = _contentStorageManager
                .Get<ContentTypeRecord>(contentTypeId);

            if (result != null && result.Name.Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
                return result;
            }

            // invalidate the cache entry and load it again
            return AcquireContentTypeRecord(contentType);
        }
    }
}
