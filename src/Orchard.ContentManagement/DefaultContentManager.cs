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
    public class DefaultContentManager : IContentManager
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly DefaultContentManagerSession _contentManagerSession;
        private readonly LinearBlockIdGenerator _idGenerator;

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentHandler> handlers,
            ISession session,
            ILoggerFactory loggerFactory,
            LinearBlockIdGenerator idGenerator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            //_contentManagerSession = contentManagerSession;
            Handlers = handlers;
            _session = session;
            _contentManagerSession = new DefaultContentManagerSession();
            _idGenerator = idGenerator;
            _logger = loggerFactory.CreateLogger<DefaultContentManager>();
        }

        public IEnumerable<IContentHandler> Handlers { get; private set; }

        public IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions()
        {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public virtual ContentItem New(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null)
            {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext
            {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

            // invoke handlers to weld aspects onto kernel
            Handlers.Invoke(handler => handler.Activating(context), _logger);

            var context2 = new ActivatedContentContext
            {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            context2.ContentItem.ContentItemId = (int)_idGenerator.GetNextId();

            Handlers.Invoke(handler => handler.Activated(context2), _logger);

            var context3 = new InitializingContentContext
            {
                ContentType = context2.ContentType,
                ContentItem = context2.ContentItem,
            };

            Handlers.Invoke(handler => handler.Initializing(context3), _logger);
            Handlers.Invoke(handler => handler.Initialized(context3), _logger);

            // composite result is returned
            return context3.ContentItem;
        }

        public async Task<ContentItem> Get(int contentItemId)
        {
            return await Get(contentItemId, VersionOptions.Published);
        }

        public async Task<ContentItem> Get(int contentItemId, VersionOptions options)
        {
            ContentItem contentItem;

            // obtain the root records based on version options
            if (options.VersionRecordId != 0)
            {
                if (_contentManagerSession.RecallVersionId(options.VersionRecordId, out contentItem))
                {
                    return contentItem;
                }

                contentItem = await _session.GetAsync<ContentItem>(options.VersionRecordId);
            }
            else if (options.VersionNumber != 0)
            {
                if (_contentManagerSession.RecallContentItemId(contentItemId, options.VersionNumber, out contentItem))
                {
                    return contentItem;
                }

                contentItem = await _session
                    .QueryAsync<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItemId &&
                        x.Number == options.VersionNumber
                    )
                    .FirstOrDefault();
            }
            else if (_contentManagerSession.RecallPublishedItemId(contentItemId, out contentItem))
            {
                if (options.IsPublished)
                {
                    return contentItem;
                }
            }
            else if (options.IsLatest)
            {
                contentItem = await _session
                    .QueryAsync<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId == contentItemId && x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsDraft && !options.IsDraftRequired)
            {
                contentItem = await _session
                    .QueryAsync<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItemId &&
                        x.Published == false &&
                        x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsDraft || options.IsDraftRequired)
            {
                contentItem = await _session
                    .QueryAsync<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItemId &&
                        x.Latest == true)
                    .FirstOrDefault();
            }
            else if (options.IsPublished)
            {
                contentItem = await _session
                    .QueryAsync<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId == contentItemId && x.Published == true)
                    .FirstOrDefault();
            }

            if (contentItem == null)
            {
                if (!options.IsDraftRequired)
                {
                    return null;
                }
            }

            // store in session prior to loading to avoid some problems with simple circular dependencies
            _contentManagerSession.Store(contentItem);

            // create a context with a new instance to load
            var context = new LoadContentContext(contentItem);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Loading(context), _logger);
            Handlers.Invoke(handler => handler.Loaded(context), _logger);

            // when draft is required and latest is published a new version is appended
            if (options.IsDraftRequired && contentItem.Published)
            {
                contentItem = await BuildNewVersionAsync(context.ContentItem);
            }

            return contentItem;
        }

        public async Task Publish(ContentItem contentItem)
        {
            if (contentItem.Published)
            {
                return;
            }

            // create a context for the item and it's previous published record
            var previous = await _session
                .QueryAsync<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == contentItem.ContentItemId && x.Published)
                .FirstOrDefault();

            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Publishing(context), _logger);

            if (context.Cancel)
            {
                return;
            }

            if (previous != null)
            {
                previous.Published = false;
            }

            contentItem.Published = true;

            Handlers.Invoke(handler => handler.Published(context), _logger);
        }

        public async Task Unpublish(ContentItem contentItem)
        {
            ContentItem publishedItem;
            if (contentItem.Published)
            {
                // The version passed in is the published one
                publishedItem = contentItem;
            }
            else
            {
                // Try to locate the published version of this item
                publishedItem = await Get(contentItem.ContentItemId, VersionOptions.Published);
            }

            if (publishedItem == null)
            {
                // No published version exists. no work to perform.
                return;
            }

            // Create a context for the item. the publishing version is null in this case
            // and the previous version is the one active prior to unpublishing. handlers
            // should take this null check into account
            var context = new PublishContentContext(contentItem, publishedItem)
            {
                PublishingItem = null
            };

            Handlers.Invoke(handler => handler.Unpublishing(context), _logger);

            publishedItem.Published = false;

            Handlers.Invoke(handler => handler.Unpublished(context), _logger);
        }

        protected async Task<ContentItem> BuildNewVersionAsync(ContentItem existingContentItem)
        {
            var buildingContentItem = New(existingContentItem.ContentType);

            var latestVersion = await _session
                .QueryAsync<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == existingContentItem.ContentItemId &&
                    x.Latest)
                .FirstOrDefault();

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                buildingContentItem.Number = latestVersion.Number + 1;
            }
            else
            {
                buildingContentItem.Number = 1;
            }

            buildingContentItem.ContentItemId = existingContentItem.ContentItemId;

            var context = new VersionContentContext
            {
                Id = existingContentItem.ContentItemId,
                ContentType = existingContentItem.ContentType,
                ExistingContentItem = existingContentItem,
                BuildingContentItem = buildingContentItem,
            };

            Handlers.Invoke(handler => handler.Versioning(context), _logger);
            Handlers.Invoke(handler => handler.Versioned(context), _logger);

            return context.BuildingContentItem;
        }

        public virtual void Create(ContentItem contentItem)
        {
            Create(contentItem, VersionOptions.Published);
        }

        public virtual void Create(ContentItem contentItem, VersionOptions options)
        {
            if (contentItem.Number == 0)
            {
                contentItem.Number = 1;
                contentItem.Latest = true;
                contentItem.Published = true;
            }

            // Version may be specified
            if (options.VersionNumber != 0)
            {
                contentItem.Number = options.VersionNumber;
            }

            // Draft flag on create is required for explicitly-published content items
            if (options.IsDraft)
            {
                contentItem.Published = false;
            }

            // Build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);

            // invoke handlers to add information to persistent stores
            Handlers.Invoke(handler => handler.Creating(context), _logger);

            Handlers.Invoke(handler => handler.Created(context), _logger);

            if (options.IsPublished)
            {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Publishing(publishContext), _logger);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Published(publishContext), _logger);
            }

            _session.Save(contentItem);
            _contentManagerSession.Store(contentItem);
        }
    }
}