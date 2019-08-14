using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.CompiledQueries;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentManagement
{
    public class DefaultContentManager : IContentManager
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly DefaultContentManagerSession _contentManagerSession;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentHandler> handlers,
            ISession session,
            IContentItemIdGenerator idGenerator,
            ILogger<DefaultContentManager> logger,
            IClock clock)
        {
            _contentDefinitionManager = contentDefinitionManager;
            Handlers = handlers;
            ReversedHandlers = handlers.Reverse().ToArray();
            _session = session;
            _idGenerator = idGenerator;
            _contentManagerSession = new DefaultContentManagerSession();
            _logger = logger;
            _clock = clock;
        }

        public IEnumerable<IContentHandler> Handlers { get; private set; }
        public IEnumerable<IContentHandler> ReversedHandlers { get; private set; }

        public async Task<ContentItem> NewAsync(string contentType)
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
            await Handlers.InvokeAsync(handler => handler.ActivatingAsync(context), _logger);

            var context2 = new ActivatedContentContext(context.Builder.Build());

            context2.ContentItem.ContentItemId = _idGenerator.GenerateUniqueId(context2.ContentItem);

            await ReversedHandlers.InvokeAsync(handler => handler.ActivatedAsync(context2), _logger);

            var context3 = new InitializingContentContext(context2.ContentItem);

            await Handlers.InvokeAsync(handler => handler.InitializingAsync(context3), _logger);
            await ReversedHandlers.InvokeAsync(handler => handler.InitializedAsync(context3), _logger);

            // composite result is returned
            return context3.ContentItem;
        }

        public Task<ContentItem> GetAsync(string contentItemId)
        {
            if (contentItemId == null)
            {
                throw new ArgumentNullException(nameof(contentItemId));
            }

            return GetAsync(contentItemId, VersionOptions.Published);
        }

        public async Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false)
        {
            if (contentItemIds == null)
            {
                throw new ArgumentNullException(nameof(contentItemIds));
            }

            List<ContentItem> contentItems;

            if (latest)
            {
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(contentItemIds) && x.Latest == true)
                    .ListAsync()).ToList();
            }
            else
            {
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(contentItemIds) && x.Published == true)
                    .ListAsync()).ToList();
            }

            for (var i = 0; i < contentItems.Count; i++)
            {
                contentItems[i] = await LoadAsync(contentItems[i]);
            }

            return contentItems.OrderBy(c => contentItemIds.ToImmutableArray().IndexOf(c.ContentItemId));
        }

        public async Task<ContentItem> GetAsync(string contentItemId, VersionOptions options)
        {
            ContentItem contentItem = null;

            if (options.IsLatest)
            {
                contentItem = await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId == contentItemId && x.Latest == true)
                    .FirstOrDefaultAsync();
            }
            else if (options.IsDraft && !options.IsDraftRequired)
            {
                contentItem = await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItemId &&
                        x.Published == false &&
                        x.Latest == true)
                    .FirstOrDefaultAsync();
            }
            else if (options.IsDraft || options.IsDraftRequired)
            {
                // Loaded whatever is the latest as it will be cloned
                contentItem = await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItemId &&
                        x.Latest == true)
                    .FirstOrDefaultAsync();
            }
            else if (options.IsPublished)
            {
                // If the published version is requested and is already loaded, we can
                // return it right away
                if (_contentManagerSession.RecallPublishedItemId(contentItemId, out contentItem))
                {
                    return contentItem;
                }

                contentItem = await _session.ExecuteQuery(new PublishedContentItemById(contentItemId)).FirstOrDefaultAsync();
            }

            if (contentItem == null)
            {
                return null;
            }

            contentItem = await LoadAsync(contentItem);

            if (options.IsDraftRequired)
            {
                // When draft is required and latest is published a new version is added
                if (contentItem.Published)
                {
                    // Save the previous version
                    _session.Save(contentItem);

                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // Check if not versionable, meaning we use only one version
                    if (!(contentTypeDefinition?.Settings.ToObject<ContentTypeSettings>().Versionable ?? true))
                    {
                        contentItem.Published = false;
                    }
                    else
                    {
                        contentItem = await BuildNewVersionAsync(contentItem);
                    }
                }

                // Save the new version
                _session.Save(contentItem);
            }

            return contentItem;
        }

        public async Task<ContentItem> LoadAsync(ContentItem contentItem)
        {
            if (!_contentManagerSession.RecallVersionId(contentItem.Id, out var loaded))
            {
                // store in session prior to loading to avoid some problems with simple circular dependencies
                _contentManagerSession.Store(contentItem);

                // create a context with a new instance to load
                var context = new LoadContentContext(contentItem);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                await Handlers.InvokeAsync(handler => handler.LoadingAsync(context), _logger);
                await ReversedHandlers.InvokeAsync(handler => handler.LoadedAsync(context), _logger);

                loaded = context.ContentItem;
            }

            return loaded;
        }

        public async Task<ContentItem> GetVersionAsync(string contentItemVersionId)
        {
            var contentItem = await _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId == contentItemVersionId)
                .FirstOrDefaultAsync();

            if (contentItem == null)
            {
                return null;
            }

            return await LoadAsync(contentItem);
        }

        public async Task PublishAsync(ContentItem contentItem)
        {
            if (contentItem.Published)
            {
                return;
            }

            // Create a context for the item and it's previous published record
            // Because of this query the content item will need to be re-enlisted
            // to be saved.
            var previous = await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == contentItem.ContentItemId && x.Published)
                .FirstOrDefaultAsync();

            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            await Handlers.InvokeAsync(handler => handler.PublishingAsync(context), _logger);

            if (context.Cancel)
            {
                return;
            }

            if (previous != null)
            {
                _session.Save(previous);
                previous.Published = false;
            }

            contentItem.Published = true;

            _session.Save(contentItem);

            await ReversedHandlers.InvokeAsync(handler => handler.PublishedAsync(context), _logger);
        }

        public async Task UnpublishAsync(ContentItem contentItem)
        {
            // This method needs to be called using the latest version
            if (!contentItem.Latest)
            {
                throw new InvalidOperationException("Not the latest version.");
            }

            ContentItem publishedItem;
            if (contentItem.Published)
            {
                // The version passed in is the published one
                publishedItem = contentItem;
            }
            else
            {
                // Try to locate the published version of this item
                publishedItem = await GetAsync(contentItem.ContentItemId, VersionOptions.Published);
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

            await Handlers.InvokeAsync(handler => handler.UnpublishingAsync(context), _logger);

            publishedItem.Published = false;
            publishedItem.ModifiedUtc = _clock.UtcNow;

            _session.Save(publishedItem);

            await ReversedHandlers.InvokeAsync(handler => handler.UnpublishedAsync(context), _logger);
        }

        protected async Task<ContentItem> BuildNewVersionAsync(ContentItem existingContentItem)
        {
            ContentItem latestVersion;

            if (existingContentItem.Latest)
            {
                latestVersion = existingContentItem;
            }
            else
            {
                latestVersion = await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentItemId == existingContentItem.ContentItemId &&
                        x.Latest)
                    .FirstOrDefaultAsync();

                if (latestVersion != null)
                {
                    _session.Save(latestVersion);
                }
            }

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
            }

            // We are not invoking NewAsync as we are cloning an existing item
            // This will also prevent the Elements (parts) from being allocated unnecessarily
            var buildingContentItem = new ContentItem();

            buildingContentItem.ContentType = existingContentItem.ContentType;
            buildingContentItem.ContentItemId = existingContentItem.ContentItemId;
            buildingContentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(existingContentItem);
            buildingContentItem.DisplayText = existingContentItem.DisplayText;
            buildingContentItem.Latest = true;
            buildingContentItem.Data = new JObject(existingContentItem.Data);

            var context = new VersionContentContext(existingContentItem, buildingContentItem);

            await Handlers.InvokeAsync(handler => handler.VersioningAsync(context), _logger);
            await ReversedHandlers.InvokeAsync(handler => handler.VersionedAsync(context), _logger);

            return context.BuildingContentItem;
        }

        public async Task CreateAsync(ContentItem contentItem, VersionOptions options)
        {
            if (String.IsNullOrEmpty(contentItem.ContentItemVersionId))
            {
                contentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(contentItem);
                contentItem.Published = true;
                contentItem.Latest = true;
            }

            // Draft flag on create is required for explicitly-published content items
            if (options.IsDraft)
            {
                contentItem.Published = false;
            }

            // Build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);

            // invoke handlers to add information to persistent stores
            await Handlers.InvokeAsync(handler => handler.CreatingAsync(context), _logger);

            await ReversedHandlers.InvokeAsync(handler => handler.CreatedAsync(context), _logger);

            _session.Save(contentItem);
            _contentManagerSession.Store(contentItem);

            if (options.IsPublished)
            {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                await Handlers.InvokeAsync(handler => handler.PublishingAsync(publishContext), _logger);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                await ReversedHandlers.InvokeAsync(handler => handler.PublishedAsync(publishContext), _logger);
            }
        }

        public async Task UpdateAsync(ContentItem contentItem)
        {
            var context = new UpdateContentContext(contentItem);

            await Handlers.InvokeAsync(handler => handler.UpdatingAsync(context), _logger);
            await ReversedHandlers.InvokeAsync(handler => handler.UpdatedAsync(context), _logger);

            _session.Save(contentItem);
        }

        public async Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
        {
            var context = new ContentItemAspectContext
            {
                ContentItem = content.ContentItem,
                Aspect = aspect
            };

            await Handlers.InvokeAsync(handler => handler.GetContentItemAspectAsync(context), _logger);

            return aspect;
        }

        public async Task RemoveAsync(ContentItem contentItem)
        {
            var activeVersions = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(x =>
                    x.ContentItemId == contentItem.ContentItemId &&
                    (x.Published || x.Latest)).ListAsync();

            var context = new RemoveContentContext(contentItem, true);

            await Handlers.InvokeAsync(handler => handler.RemovingAsync(context), _logger);

            foreach (var version in activeVersions)
            {
                version.Published = false;
                version.Latest = false;
                _session.Save(version);
            }

            await ReversedHandlers.InvokeAsync(handler => handler.RemovedAsync(context), _logger);
        }

        public async Task DiscardDraftAsync(ContentItem contentItem)
        {
            if (contentItem.Published || !contentItem.Latest)
            {
                throw new InvalidOperationException("Not a draft version.");
            }

            var publishedItem = await GetAsync(contentItem.ContentItemId, VersionOptions.Published);

            var context = new RemoveContentContext(contentItem, publishedItem == null);

            await Handlers.InvokeAsync(handler => handler.RemovingAsync(context), _logger);

            contentItem.Latest = false;
            _session.Save(contentItem);

            await ReversedHandlers.InvokeAsync(handler => handler.RemovedAsync(context), _logger);


            if (publishedItem != null)
            {
                publishedItem.Latest = true;
                _session.Save(publishedItem);
            }
        }

        public async Task<ContentItem> CloneAsync(ContentItem contentItem)
        {
            var cloneContentItem = await NewAsync(contentItem.ContentType);
            await CreateAsync(cloneContentItem, VersionOptions.Draft);

            var context = new CloneContentContext(contentItem, cloneContentItem);

            context.CloneContentItem.Data = contentItem.Data.DeepClone() as JObject;
            context.CloneContentItem.DisplayText = contentItem.DisplayText;

            await Handlers.InvokeAsync(handler => handler.CloningAsync(context), _logger);
            await ReversedHandlers.InvokeAsync(handler => handler.ClonedAsync(context), _logger);

            _session.Save(context.CloneContentItem);
            return context.CloneContentItem;
        }
    }
}
