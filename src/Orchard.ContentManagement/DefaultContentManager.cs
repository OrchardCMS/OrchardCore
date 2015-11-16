using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Microsoft.Extensions.Logging;
using YesSql.Core.Services;
using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public class DefaultContentManager : IContentManager {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        //private readonly IContentManagerSession _contentManagerSession;
        private readonly ISession _session; 
        private readonly ILogger _logger;
        private readonly DefaultContentManagerSession _contentManagerSession;

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            //IContentManagerSession contentManagerSession,
            IEnumerable<IContentHandler> handlers,
            ISession session,
            ILoggerFactory loggerFactory) {
            _contentDefinitionManager = contentDefinitionManager;
            //_contentManagerSession = contentManagerSession;
            Handlers = handlers;
            _session = session;
            _contentManagerSession = new DefaultContentManagerSession();
            _logger = loggerFactory.CreateLogger<DefaultContentManager>();
        }

        public IEnumerable<IContentHandler> Handlers { get; private set; }

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
            Handlers.Invoke(handler => handler.Activating(context), _logger);

            var context2 = new ActivatedContentContext {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            Handlers.Invoke(handler => handler.Activated(context2), _logger);

            var context3 = new InitializingContentContext {
                ContentType = context2.ContentType,
                ContentItem = context2.ContentItem,
            };

            Handlers.Invoke(handler => handler.Initializing(context3), _logger);
            Handlers.Invoke(handler => handler.Initialized(context3), _logger);

            // composite result is returned
            return context3.ContentItem;
        }

        public async Task<ContentItem> Get(int id) {
            return await Get(id, VersionOptions.Published);
        }

        public async Task<ContentItem> Get(int id, VersionOptions options) {
            ContentItem contentItem;

            ContentItemVersionRecord versionRecord = null;

            // obtain the root records based on version options
            if (options.VersionRecordId != 0) {

                if (_contentManagerSession.RecallVersionRecordId(options.VersionRecordId, out contentItem))
                {
                    return contentItem;
                }

                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x => x.Id == options.VersionRecordId)
                    .FirstOrDefault();
            }
            else if (options.VersionNumber != 0) {
                if (_contentManagerSession.RecallVersionNumber(id, options.VersionNumber, out contentItem))
                {
                    return contentItem;
                }

                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x =>
                        x.ContentItemId == id &&
                        x.Number == options.VersionNumber
                    )
                    .FirstOrDefault();

            }
            else if (_contentManagerSession.RecallContentRecordId(id, out contentItem))
            {
                // try to reload a previously loaded published content item

                if (options.IsPublished)
                {
                    return contentItem;
                }

                versionRecord = contentItem.VersionRecord;
            }
            else if (options.IsLatest)
            {
                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x => x.ContentItemId == id && x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsDraft && !options.IsDraftRequired)
            {
                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x =>
                        x.ContentItemId == id &&
                        x.Published == false &&
                        x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsDraft || options.IsDraftRequired)
            {
                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x => x.ContentItemId == id && x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsPublished)
            {
                versionRecord = await _session
                    .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>()
                    .Where(x => x.ContentItemId == id && x.Published == true)
                    .FirstOrDefault();
            }
                
            if(versionRecord == null && !options.IsDraftRequired)
            {
                return null;
            }
            
            var record = await _session.GetAsync<ContentItemRecord>(id);

            // allocate instance and set record property
            contentItem = New(versionRecord.ContentType);
            contentItem.Record = record;
            contentItem.VersionRecord = versionRecord;

            // store in session prior to loading to avoid some problems with simple circular dependencies
            _contentManagerSession.Store(contentItem);

            // create a context with a new instance to load            
            var context = new LoadContentContext(contentItem);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Loading(context), _logger);
            Handlers.Invoke(handler => handler.Loaded(context), _logger);

            // when draft is required and latest is published a new version is appended 
            if (options.IsDraftRequired && versionRecord.Published) {
                contentItem = await BuildNewVersionAsync(context.ContentItem);
            }

            return contentItem;
        }
        
        public async Task Publish(ContentItem contentItem) {
            if (contentItem.VersionRecord.Published) {
                return;
            }
            // create a context for the item and it's previous published record
            var previous = await _session
                .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>(x => 
                    x.ContentItemId == contentItem.Id && x.Published)
                .FirstOrDefault();

            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Publishing(context), _logger);

            if (context.Cancel) {
                return;
            }

            if (previous != null) {
                previous.Published = false;
            }
            contentItem.VersionRecord.Published = true;

            Handlers.Invoke(handler => handler.Published(context), _logger);
        }

        public async Task Unpublish(ContentItem contentItem) {
            ContentItem publishedItem;
            if (contentItem.VersionRecord.Published) {
                // the version passed in is the published one
                publishedItem = contentItem;
            }
            else {
                // try to locate the published version of this item
                publishedItem = await Get(contentItem.Id, VersionOptions.Published);
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

            Handlers.Invoke(handler => handler.Unpublishing(context), _logger);

            publishedItem.VersionRecord.Published = false;

            Handlers.Invoke(handler => handler.Unpublished(context), _logger);
        }
        
        protected async Task<ContentItem> BuildNewVersionAsync(ContentItem existingContentItem) {
            var contentItemRecord = existingContentItem.Record;

            // locate the existing and the current latest versions, allocate building version
            var existingItemVersionRecord = existingContentItem.VersionRecord;
            var buildingItemVersionRecord = new ContentItemVersionRecord {
                ContentItemId = contentItemRecord.Id,
                Latest = true,
                Published = false,
            };


            var latestVersion = await _session
                .QueryAsync<ContentItemVersionRecord, ContentItemVersionRecordIndex>(x => 
                    x.ContentItemId == contentItemRecord.Id && x.Latest)
                .FirstOrDefault();

            if (latestVersion != null) {
                latestVersion.Latest = false;
                buildingItemVersionRecord.Number = latestVersion.Number + 1;
            }
            else {
                var biggest = await _session.QueryIndexAsync<ContentItemVersionRecordIndex>(x => 
                x.ContentItemId == contentItemRecord.Id)
                .OrderByDescending(x => x.Number)
                .FirstOrDefault();
                buildingItemVersionRecord.Number = biggest.Number + 1;
            }

            buildingItemVersionRecord.ContentItemId = contentItemRecord.Id;
            _session.Save(buildingItemVersionRecord);

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

            Handlers.Invoke(handler => handler.Versioning(context), _logger);
            Handlers.Invoke(handler => handler.Versioned(context), _logger);

            return context.BuildingContentItem;
        }

        public virtual void Create(ContentItem contentItem) {
            Create(contentItem, VersionOptions.Published);
        }

        public virtual void Create(ContentItem contentItem, VersionOptions options) {
            if (contentItem.VersionRecord == null) {
                // produce root record to determine the model id
                contentItem.VersionRecord = new ContentItemVersionRecord {
                    Number = 1,
                    Latest = true,
                    Published = true
                };
            }

            // add to the collection manually for the created case
            contentItem.VersionRecord.ContentType = contentItem.ContentType;

            // version may be specified
            if (options.VersionNumber != 0) {
                contentItem.VersionRecord.Number = options.VersionNumber;
            }

            // draft flag on create is required for explicitly-published content items
            if (options.IsDraft) {
                contentItem.VersionRecord.Published = false;
            }

            // build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);

            // invoke handlers to add information to persistent stores
            Handlers.Invoke(handler => handler.Creating(context), _logger);

            Handlers.Invoke(handler => handler.Created(context), _logger);

            if (options.IsPublished) {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Publishing(publishContext), _logger);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Published(publishContext), _logger);
            }

            _session.Save(contentItem.Record);
            contentItem.VersionRecord.ContentItemId = contentItem.Record.Id;
            _session.Save(contentItem.VersionRecord);

            _contentManagerSession.Store(contentItem);
        }
    }
}
