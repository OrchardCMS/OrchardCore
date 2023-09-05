using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.CompiledQueries;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentManagement
{
    public class DefaultContentManager : IContentManager
    {
        private const int ImportBatchSize = 500;
        private static readonly JsonMergeSettings _updateJsonMergeSettings = new() { MergeArrayHandling = MergeArrayHandling.Replace };

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IContentManagerSession contentManagerSession,
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
            _contentManagerSession = contentManagerSession;
            _logger = logger;
            _clock = clock;
        }

        public IEnumerable<IContentHandler> Handlers { get; private set; }
        public IEnumerable<IContentHandler> ReversedHandlers { get; private set; }

        public async Task<ContentItem> NewAsync(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            contentTypeDefinition ??= new ContentTypeDefinitionBuilder().Named(contentType).Build();

            // Create a new kernel for the model instance.
            var context = new ActivatingContentContext(new ContentItem() { ContentType = contentTypeDefinition.Name })
            {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
            };

            // Invoke handlers to weld aspects onto kernel.
            await Handlers.InvokeAsync((handler, context) => handler.ActivatingAsync(context), context, _logger);

            var context2 = new ActivatedContentContext(context.ContentItem);

            context2.ContentItem.ContentItemId = _idGenerator.GenerateUniqueId(context2.ContentItem);

            await ReversedHandlers.InvokeAsync((handler, context2) => handler.ActivatedAsync(context2), context2, _logger);

            var context3 = new InitializingContentContext(context2.ContentItem);

            await Handlers.InvokeAsync((handler, context3) => handler.InitializingAsync(context3), context3, _logger);
            await ReversedHandlers.InvokeAsync((handler, context3) => handler.InitializedAsync(context3), context3, _logger);

            // Composite result is returned.
            return context3.ContentItem;
        }

        public Task<ContentItem> GetAsync(string contentItemId)
        {
            return GetAsync(contentItemId, VersionOptions.Published);
        }

        public async Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false)
        {
            var itemIds = contentItemIds
                ?.Where(id => id is not null)
                .Distinct()
                .ToArray()
                ?? throw new ArgumentNullException(nameof(contentItemIds));

            if (itemIds.Length == 0)
            {
                return Enumerable.Empty<ContentItem>();
            }

            List<ContentItem> contentItems = null;
            List<ContentItem> storedItems = null;
            if (latest)
            {
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(i => i.ContentItemId.IsIn(itemIds) && i.Latest == true)
                    .ListAsync()
                    ).ToList();
            }
            else
            {
                foreach (var itemId in itemIds)
                {
                    // If the published version is already stored, we can return it.
                    if (_contentManagerSession.RecallPublishedItemId(itemId, out var contentItem))
                    {
                        storedItems ??= new List<ContentItem>();
                        storedItems.Add(contentItem);
                    }
                }

                // Only query the ids not already stored.
                var itemIdsToQuery = storedItems is not null
                    ? itemIds.Except(storedItems.Select(c => c.ContentItemId)).ToArray()
                    : itemIds;

                if (itemIdsToQuery.Length > 0)
                {
                    contentItems = (await _session
                       .Query<ContentItem, ContentItemIndex>()
                       .Where(i => i.ContentItemId.IsIn(itemIdsToQuery) && i.Published == true)
                       .ListAsync()
                       ).ToList();
                }
            }

            if (contentItems is not null)
            {
                for (var i = 0; i < contentItems.Count; i++)
                {
                    contentItems[i] = await LoadAsync(contentItems[i]);
                }

                if (storedItems is not null)
                {
                    contentItems.AddRange(storedItems);
                }
            }
            else if (storedItems is not null)
            {
                contentItems = storedItems;
            }
            else
            {
                return Enumerable.Empty<ContentItem>();
            }

            return contentItems.OrderBy(c => Array.IndexOf(itemIds, c.ContentItemId));
        }

        public async Task<ContentItem> GetAsync(string contentItemId, VersionOptions options)
        {
            if (String.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

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
                    // We save the previous version further because this call might do a session query.
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // Check if not versionable, meaning we use only one version
                    if (contentTypeDefinition != null && !contentTypeDefinition.IsVersionable())
                    {
                        contentItem.Published = false;
                    }
                    else
                    {
                        // Save the previous version
                        _session.Save(contentItem, checkConcurrency: true);

                        contentItem = await BuildNewVersionAsync(contentItem);
                    }
                }

                // Save the new version
                _session.Save(contentItem, checkConcurrency: true);
            }

            return contentItem;
        }

        public async Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, VersionOptions options)
        {
            if (contentItemIds == null || !contentItemIds.Any())
            {
                return Enumerable.Empty<ContentItem>();
            }

            var ids = new List<string>(contentItemIds);

            var contentItems = new List<ContentItem>();

            if (options.IsLatest)
            {
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(ids) && x.Latest)
                    .ListAsync()
                    ).ToList();
            }
            else if (options.IsDraft && !options.IsDraftRequired)
            {
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(ids) && !x.Published && x.Latest).ListAsync()
                    ).ToList();
            }
            else if (options.IsDraft || options.IsDraftRequired)
            {
                // Loaded whatever is the latest as it will be cloned
                contentItems = (await _session
                    .Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(ids) && x.Latest)
                    .ListAsync()).ToList();
            }
            else if (options.IsPublished)
            {
                var missingIds = new List<string>();

                foreach (var id in ids)
                {
                    if (_contentManagerSession.RecallPublishedItemId(id, out var contentItem))
                    {
                        contentItems.Add(contentItem);
                    }
                    else
                    {
                        missingIds.Add(id);
                    }
                }

                if (missingIds.Count > 0)
                {
                    var missingItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId.IsIn(missingIds) && x.Published).ListAsync();

                    contentItems.AddRange(missingItems);
                }
            }

            var needVersions = new List<ContentItem>();
            var finalItems = new List<ContentItem>();

            foreach (var contentItem in contentItems)
            {
                var item = await LoadAsync(contentItem);

                if (options.IsDraftRequired)
                {
                    // When draft is required and latest is published a new version is added.
                    if (item.Published)
                    {
                        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(item.ContentType);

                        // Check if not versionable, meaning we use only one version.
                        if (contentTypeDefinition != null && !contentTypeDefinition.IsVersionable())
                        {
                            item.Published = false;

                            // Save the previous version.
                            _session.Save(item, checkConcurrency: true);

                            finalItems.Add(item);
                        }
                        else
                        {
                            needVersions.Add(item);
                        }
                    }
                }
                else
                {
                    finalItems.Add(item);
                }

                // We save the previous version further because this call might do a session query.
                _session.Save(item, checkConcurrency: true);
            }

            if (needVersions.Count > 0)
            {
                var items = await BuildNewVersionsAsync(needVersions);

                foreach (var item in items)
                {
                    // Save the new version.
                    _session.Save(item, checkConcurrency: true);

                    finalItems.Add(item);
                }
            }

            return finalItems;
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
                await Handlers.InvokeAsync((handler, context) => handler.LoadingAsync(context), context, _logger);
                await ReversedHandlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), context, _logger);

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

        public async Task SaveDraftAsync(ContentItem contentItem)
        {
            if (!contentItem.Latest || contentItem.Published)
            {
                return;
            }

            var context = new SaveDraftContentContext(contentItem);

            await Handlers.InvokeAsync((handler, context) => handler.DraftSavingAsync(context), context, _logger);

            _session.Save(contentItem, checkConcurrency: true);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.DraftSavedAsync(context), context, _logger);
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
            await Handlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), context, _logger);

            if (context.Cancel)
            {
                return;
            }

            if (previous != null)
            {
                _session.Save(previous, checkConcurrency: true);
                previous.Published = false;
            }

            contentItem.Published = true;
            _session.Save(contentItem, checkConcurrency: true);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), context, _logger);
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

            await Handlers.InvokeAsync((handler, context) => handler.UnpublishingAsync(context), context, _logger);

            publishedItem.Published = false;
            publishedItem.ModifiedUtc = _clock.UtcNow;
            _session.Save(publishedItem, checkConcurrency: true);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.UnpublishedAsync(context), context, _logger);
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
            var buildingContentItem = new ContentItem
            {
                ContentType = existingContentItem.ContentType,
                ContentItemId = existingContentItem.ContentItemId,
                ContentItemVersionId = _idGenerator.GenerateUniqueId(existingContentItem),
                DisplayText = existingContentItem.DisplayText,
                Latest = true,
                Data = new JObject(existingContentItem.Data),
            };

            var context = new VersionContentContext(existingContentItem, buildingContentItem);

            await Handlers.InvokeAsync((handler, context) => handler.VersioningAsync(context), context, _logger);
            await ReversedHandlers.InvokeAsync((handler, context) => handler.VersionedAsync(context), context, _logger);

            return context.BuildingContentItem;
        }

        protected async Task<IEnumerable<ContentItem>> BuildNewVersionsAsync(IEnumerable<ContentItem> existingContentItems)
        {
            var latestVersions = new List<ContentItem>();
            var needingLatestVersion = new List<string>();

            foreach (var existingContentItem in existingContentItems)
            {
                if (existingContentItem.Latest)
                {
                    latestVersions.Add(existingContentItem);

                    continue;
                }

                needingLatestVersion.Add(existingContentItem.ContentItemId);
            }

            if (needingLatestVersion.Count > 0)
            {
                var foundLatestVersions = await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentItemId.IsIn(needingLatestVersion) &&
                        x.Latest)
                    .ListAsync();

                latestVersions.AddRange(foundLatestVersions);
            }

            var finalVersions = new List<ContentItem>();

            foreach (var existingContentItem in latestVersions)
            {
                existingContentItem.Latest = false;

                // Save previous version.
                _session.Save(existingContentItem);

                // We are not invoking NewAsync as we are cloning an existing item
                // This will also prevent the Elements (parts) from being allocated unnecessarily
                var buildingContentItem = new ContentItem
                {
                    ContentType = existingContentItem.ContentType,
                    ContentItemId = existingContentItem.ContentItemId,
                    ContentItemVersionId = _idGenerator.GenerateUniqueId(existingContentItem),
                    DisplayText = existingContentItem.DisplayText,
                    Latest = true,
                    Data = new JObject(existingContentItem.Data),
                };

                var context = new VersionContentContext(existingContentItem, buildingContentItem);

                await Handlers.InvokeAsync((handler, context) => handler.VersioningAsync(context), context, _logger);
                await ReversedHandlers.InvokeAsync((handler, context) => handler.VersionedAsync(context), context, _logger);

                finalVersions.Add(context.BuildingContentItem);
            }

            return finalVersions;
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
            await Handlers.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger);

            _session.Save(contentItem);
            _contentManagerSession.Store(contentItem);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);

            if (options.IsPublished)
            {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                await Handlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                await ReversedHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger);
            }
        }

        public Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem)
        {
            return CreateContentItemVersionAsync(contentItem, null);
        }

        public Task<ContentValidateResult> UpdateContentItemVersionAsync(ContentItem updatingVersion, ContentItem updatedVersion)
        {
            return UpdateContentItemVersionAsync(updatingVersion, updatedVersion, null);
        }

        public async Task ImportAsync(IEnumerable<ContentItem> contentItems)
        {
            var skip = 0;

            var importedVersionIds = new HashSet<string>();

            var batchedContentItems = contentItems.Take(ImportBatchSize);

            while (batchedContentItems.Any())
            {
                // Preload all the versions for this batch from the database.
                var versionIds = batchedContentItems
                     .Where(x => !String.IsNullOrEmpty(x.ContentItemVersionId))
                     .Select(x => x.ContentItemVersionId);

                var itemIds = batchedContentItems
                    .Where(x => !String.IsNullOrEmpty(x.ContentItemId))
                    .Select(x => x.ContentItemId);

                var existingContentItems = await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentItemId.IsIn(itemIds) &&
                        (x.Latest || x.Published || x.ContentItemVersionId.IsIn(versionIds)))
                    .ListAsync();

                var versionsToUpdate = existingContentItems.Where(c => versionIds.Any(v => String.Equals(v, c.ContentItemVersionId, StringComparison.OrdinalIgnoreCase)));
                var versionsThatMaybeEvicted = existingContentItems.Except(versionsToUpdate);

                foreach (var version in existingContentItems)
                {
                    await LoadAsync(version);
                }

                foreach (var importingItem in batchedContentItems)
                {
                    ContentItem originalVersion = null;
                    if (!String.IsNullOrEmpty(importingItem.ContentItemVersionId))
                    {
                        if (importedVersionIds.Contains(importingItem.ContentItemVersionId))
                        {
                            _logger.LogInformation("Duplicate content item version id '{ContentItemVersionId}' skipped", importingItem.ContentItemVersionId);
                            continue;
                        }

                        importedVersionIds.Add(importingItem.ContentItemVersionId);

                        originalVersion = versionsToUpdate.FirstOrDefault(x => String.Equals(x.ContentItemVersionId, importingItem.ContentItemVersionId, StringComparison.OrdinalIgnoreCase));
                    }

                    if (originalVersion == null)
                    {
                        // The version does not exist in the current database.
                        var context = new ImportContentContext(importingItem);

                        await Handlers.InvokeAsync((handler, context) => handler.ImportingAsync(context), context, _logger);

                        var evictionVersions = versionsThatMaybeEvicted.Where(x => String.Equals(x.ContentItemId, importingItem.ContentItemId, StringComparison.OrdinalIgnoreCase));
                        var result = await CreateContentItemVersionAsync(importingItem, evictionVersions);
                        if (!result.Succeeded)
                        {
                            if (_logger.IsEnabled(LogLevel.Error))
                            {
                                _logger.LogError("Error importing content item version id '{ContentItemVersionId}' : '{Errors}'", importingItem?.ContentItemVersionId, String.Join(", ", result.Errors));
                            }

                            throw new ValidationException(String.Join(", ", result.Errors));
                        }

                        // Imported handlers will only be fired if the validation has been successful.
                        // Consumers should implement validated handlers to alter the success of that operation.
                        await ReversedHandlers.InvokeAsync((handler, context) => handler.ImportedAsync(context), context, _logger);
                    }
                    else
                    {
                        // The version exists in the database.
                        // It is important to only import changed items.
                        // We compare the two versions and skip importing it if they are the same.
                        // We do this to prevent unnecessary sql updates, and because UpdateContentItemVersionAsync
                        // may remove drafts of updated items.
                        // This is necesary because an imported item maybe set to latest, and published.
                        // In this case, the draft item in the system, must be removed, or there will be two drafts.
                        // The draft item should be removed, because it would now be orphaned, as the imported published item
                        // would be further ahead, on a timeline, between the two.

                        var jImporting = JObject.FromObject(importingItem);

                        // Removed Published and Latest from consideration when evaluating.
                        // Otherwise an import of an unchanged (but published) version would overwrite a newer published version.
                        jImporting.Remove(nameof(ContentItem.Published));
                        jImporting.Remove(nameof(ContentItem.Latest));

                        var jOriginal = JObject.FromObject(originalVersion);

                        jOriginal.Remove(nameof(ContentItem.Published));
                        jOriginal.Remove(nameof(ContentItem.Latest));

                        if (JToken.DeepEquals(jImporting, jOriginal))
                        {
                            _logger.LogInformation("Importing '{ContentItemVersionId}' skipped as it is unchanged", importingItem.ContentItemVersionId);
                            continue;
                        }

                        // Handlers are only fired if the import is going ahead.
                        var context = new ImportContentContext(importingItem, originalVersion);

                        await Handlers.InvokeAsync((handler, context) => handler.ImportingAsync(context), context, _logger);

                        var evictionVersions = versionsThatMaybeEvicted.Where(x => String.Equals(x.ContentItemId, importingItem.ContentItemId, StringComparison.OrdinalIgnoreCase));
                        var result = await UpdateContentItemVersionAsync(originalVersion, importingItem, evictionVersions);
                        if (!result.Succeeded)
                        {
                            if (_logger.IsEnabled(LogLevel.Error))
                            {
                                _logger.LogError("Error importing content item version id '{ContentItemVersionId}' : '{Errors}'", importingItem.ContentItemVersionId, String.Join(", ", result.Errors));
                            }

                            throw new ValidationException(String.Join(", ", result.Errors));
                        }

                        // Imported handlers will only be fired if the validation has been successful.
                        // Consumers should implement validated handlers to alter the success of that operation.
                        await ReversedHandlers.InvokeAsync((handler, context) => handler.ImportedAsync(context), context, _logger);
                    }
                }

                skip += ImportBatchSize;
                batchedContentItems = contentItems.Skip(skip).Take(ImportBatchSize);
            }
        }

        public async Task UpdateAsync(ContentItem contentItem)
        {
            var context = new UpdateContentContext(contentItem);

            await Handlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), context, _logger);

            _session.Save(contentItem);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
        }

        public async Task<ContentValidateResult> ValidateAsync(ContentItem contentItem)
        {
            var validateContext = new ValidateContentContext(contentItem);

            await Handlers.InvokeAsync((handler, context) => handler.ValidatingAsync(context), validateContext, _logger);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.ValidatedAsync(context), validateContext, _logger);

            if (!validateContext.ContentValidateResult.Succeeded)
            {
                await _session.CancelAsync();
            }

            return validateContext.ContentValidateResult;
        }

        public async Task<ContentValidateResult> RestoreAsync(ContentItem contentItem)
        {
            // Prepare record for restore.
            // So that a new record will be created.
            contentItem.Id = 0;
            // So that a new version id will be generated.
            contentItem.ContentItemVersionId = "";
            contentItem.Latest = contentItem.Published = false;

            var context = new RestoreContentContext(contentItem);
            await Handlers.InvokeAsync((handler, context) => handler.RestoringAsync(context), context, _logger);

            // Invoke save and fire update handlers.
            await UpdateAsync(contentItem);

            var validationResult = await ValidateAsync(contentItem);
            if (!validationResult.Succeeded)
            {
                // The session is already cancelled.
                return validationResult;
            }

            // Remove an existing draft but keep an existing published version.
            var latestVersion = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(index => index.ContentItemId == contentItem.ContentItemId && index.Latest)
                .FirstOrDefaultAsync();

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                _session.Save(latestVersion);
            }

            await CreateAsync(contentItem, VersionOptions.Draft);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.RestoredAsync(context), context, _logger);

            return validationResult;
        }

        public async Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
        {
            var context = new ContentItemAspectContext
            {
                ContentItem = content.ContentItem,
                Aspect = aspect
            };

            await Handlers.InvokeAsync((handler, context) => handler.GetContentItemAspectAsync(context), context, _logger);

            return aspect;
        }

        public async Task RemoveAsync(ContentItem contentItem)
        {
            var activeVersions = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(x =>
                    x.ContentItemId == contentItem.ContentItemId &&
                    (x.Published || x.Latest)).ListAsync();

            if (!activeVersions.Any())
            {
                return;
            }

            var context = new RemoveContentContext(contentItem, true);

            await Handlers.InvokeAsync((handler, context) => handler.RemovingAsync(context), context, _logger);

            foreach (var version in activeVersions)
            {
                version.Published = false;
                version.Latest = false;
                _session.Save(version);
            }

            await ReversedHandlers.InvokeAsync((handler, context) => handler.RemovedAsync(context), context, _logger);
        }

        public async Task DiscardDraftAsync(ContentItem contentItem)
        {
            if (contentItem.Published || !contentItem.Latest)
            {
                throw new InvalidOperationException("Not a draft version.");
            }

            var publishedItem = await GetAsync(contentItem.ContentItemId, VersionOptions.Published);

            var context = new RemoveContentContext(contentItem, publishedItem == null);

            await Handlers.InvokeAsync((handler, context) => handler.RemovingAsync(context), context, _logger);

            contentItem.Latest = false;
            _session.Save(contentItem);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.RemovedAsync(context), context, _logger);

            if (publishedItem != null)
            {
                publishedItem.Latest = true;
                _session.Save(publishedItem);
            }
        }

        public async Task<ContentItem> CloneAsync(ContentItem contentItem)
        {
            var cloneContentItem = await NewAsync(contentItem.ContentType);
            cloneContentItem.DisplayText = contentItem.DisplayText;
            await CreateAsync(cloneContentItem, VersionOptions.Draft);

            var context = new CloneContentContext(contentItem, cloneContentItem);

            context.CloneContentItem.Data = contentItem.Data.DeepClone() as JObject;

            await Handlers.InvokeAsync((handler, context) => handler.CloningAsync(context), context, _logger);

            _session.Save(context.CloneContentItem);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.ClonedAsync(context), context, _logger);

            return context.CloneContentItem;
        }

        private async Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem, IEnumerable<ContentItem> evictionVersions = null)
        {
            if (String.IsNullOrEmpty(contentItem.ContentItemId))
            {
                // NewAsync should be used to create new content items.
                throw new InvalidOperationException($"The content item is missing a '{nameof(ContentItem.ContentItemId)}'.");
            }

            // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
            contentItem.Id = 0;

            // Maintain modified and published dates as these will be reset by the Create Handlers
            var modifiedUtc = contentItem.ModifiedUtc;
            var publishedUtc = contentItem.PublishedUtc;
            var owner = contentItem.Owner;
            var author = contentItem.Author;

            if (String.IsNullOrEmpty(contentItem.ContentItemVersionId))
            {
                contentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(contentItem);
            }

            // Remove previous latest item or they will continue to be listed as latest.
            // When importing a new draft the existing latest must be set to false. The creating version wins.
            if (contentItem.Latest && !contentItem.Published)
            {
                await RemoveLatestVersionAsync(contentItem, evictionVersions);
            }
            else if (contentItem.Published)
            {
                // When importing a published item existing drafts and existing published must be removed.
                // Otherwise an existing draft would become an orphan and if published would overwrite
                // the imported (which we assume is the version that wins) content.
                await RemoveVersionsAsync(contentItem, evictionVersions);
            }
            // When neither published or latest the operation will create a database record
            // which will be part of the content item archive.

            // Invoked create handlers.
            var context = new CreateContentContext(contentItem);
            await Handlers.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger);

            // The content item should be placed in the session store so that further calls
            // to ContentManager.Get by a scoped index provider will resolve the imported item correctly.
            _session.Save(contentItem);
            _contentManagerSession.Store(contentItem);

            await ReversedHandlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);

            await UpdateAsync(contentItem);

            var result = await ValidateAsync(contentItem);
            if (!result.Succeeded)
            {
                return result;
            }

            if (contentItem.Published)
            {
                // Invoke published handlers to add information to persistent stores
                var publishContext = new PublishContentContext(contentItem, null);

                await Handlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger);
                await ReversedHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger);
            }
            else
            {
                await SaveDraftAsync(contentItem);
            }

            // Restore values that may have been altered by handlers.
            if (modifiedUtc.HasValue)
            {
                contentItem.ModifiedUtc = modifiedUtc;
            }
            if (publishedUtc.HasValue)
            {
                contentItem.PublishedUtc = publishedUtc;
            }

            // There is a risk here that the owner or author does not exist in the importing system.
            // We check that at least a value has been supplied, if not the owner property and author
            // property would be left as the user who has run this import.
            if (!String.IsNullOrEmpty(owner))
            {
                contentItem.Owner = owner;
            }
            if (!String.IsNullOrEmpty(author))
            {
                contentItem.Author = author;
            }

            return result;
        }

        private async Task<ContentValidateResult> UpdateContentItemVersionAsync(ContentItem updatingVersion, ContentItem updatedVersion, IEnumerable<ContentItem> evictionVersions = null)
        {
            // Replaces the id to force the current item to be updated
            updatingVersion.Id = updatedVersion.Id;

            var modifiedUtc = updatedVersion.ModifiedUtc;
            var publishedUtc = updatedVersion.PublishedUtc;

            // Remove previous published or draft items if necesary or they will continue to be listed as published or draft.
            var discardLatest = false;
            var removePublished = false;

            var importingLatest = updatedVersion.Latest;
            var existingLatest = updatingVersion.Latest;

            // If latest values do not match and importing latest is true then we must find and evict the previous latest.
            if (importingLatest != existingLatest && importingLatest == true)
            {
                discardLatest = true;
            }

            var importingPublished = updatedVersion.Published;
            var existingPublished = updatingVersion.Published;

            // If published values do not match and importing published is true then we must find and evict the previous published
            // This is when the existing content item version is not published, but the importing version is set to published.
            // For this to occur there must have been a draft made, and the mutation to published is being made on the draft.
            if (importingPublished != existingPublished && importingPublished == true)
            {
                removePublished = true;
            }

            if (discardLatest && removePublished)
            {
                await RemoveVersionsAsync(updatingVersion, evictionVersions);
            }
            else if (discardLatest)
            {
                await RemoveLatestVersionAsync(updatingVersion, evictionVersions);
            }
            else if (removePublished)
            {
                await RemovePublishedVersionAsync(updatingVersion, evictionVersions);
            }

            updatingVersion.Merge(updatedVersion, _updateJsonMergeSettings);
            updatingVersion.Latest = importingLatest;
            updatingVersion.Published = importingPublished;

            await UpdateAsync(updatingVersion);
            var result = await ValidateAsync(updatingVersion);

            // Session is cancelled now so previous updates to versions are cancelled also.
            if (!result.Succeeded)
            {
                return result;
            }

            if (importingPublished)
            {
                // Invoke published handlers to add information to persistent stores
                var publishContext = new PublishContentContext(updatingVersion, null);

                await Handlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger);
                await ReversedHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger);
            }
            else
            {
                await SaveDraftAsync(updatingVersion);
            }

            // Restore values that may have been altered by handlers.
            if (modifiedUtc.HasValue)
            {
                updatingVersion.ModifiedUtc = modifiedUtc;
            }
            if (publishedUtc.HasValue)
            {
                updatingVersion.PublishedUtc = publishedUtc;
            }

            return result;
        }

        private async Task RemoveLatestVersionAsync(ContentItem contentItem, IEnumerable<ContentItem> evictionVersions)
        {
            ContentItem latestVersion;
            if (evictionVersions == null)
            {
                latestVersion = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId == contentItem.ContentItemId && x.Latest)
                    .FirstOrDefaultAsync();
            }
            else
            {
                latestVersion = evictionVersions.FirstOrDefault(x => x.Latest);
            }

            if (latestVersion != null)
            {
                var publishedVersion = evictionVersions?.FirstOrDefault(x => x.Published);

                var removeContext = new RemoveContentContext(contentItem, publishedVersion == null);

                await Handlers.InvokeAsync((handler, context) => handler.RemovingAsync(context), removeContext, _logger);

                latestVersion.Latest = false;
                _session.Save(latestVersion);

                await ReversedHandlers.InvokeAsync((handler, context) => handler.RemovedAsync(context), removeContext, _logger);
            }
        }

        private async Task RemovePublishedVersionAsync(ContentItem contentItem, IEnumerable<ContentItem> evictionVersions)
        {
            ContentItem publishedVersion;
            if (evictionVersions == null)
            {
                publishedVersion = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId == contentItem.ContentItemId && x.Published)
                    .FirstOrDefaultAsync();
            }
            else
            {
                publishedVersion = evictionVersions.FirstOrDefault(x => x.Published);
            }

            if (publishedVersion != null)
            {
                var removeContext = new RemoveContentContext(contentItem, true);

                await Handlers.InvokeAsync((handler, context) => handler.RemovingAsync(context), removeContext, _logger);

                publishedVersion.Published = false;
                _session.Save(publishedVersion);

                await ReversedHandlers.InvokeAsync((handler, context) => handler.RemovedAsync(context), removeContext, _logger);
            }
        }

        private async Task RemoveVersionsAsync(ContentItem contentItem, IEnumerable<ContentItem> evictionVersions)
        {
            IEnumerable<ContentItem> activeVersions;
            if (evictionVersions == null)
            {
                activeVersions = await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x =>
                        x.ContentItemId == contentItem.ContentItemId &&
                        (x.Published || x.Latest)).ListAsync();
            }
            else
            {
                activeVersions = evictionVersions.Where(x => x.Latest || x.Published);
            }

            if (activeVersions.Any())
            {
                var removeContext = new RemoveContentContext(contentItem, true);

                await Handlers.InvokeAsync((handler, context) => handler.RemovingAsync(context), removeContext, _logger);

                foreach (var version in activeVersions)
                {
                    version.Published = false;
                    version.Latest = false;
                    _session.Save(version);
                }

                await ReversedHandlers.InvokeAsync((handler, context) => handler.RemovedAsync(context), removeContext, _logger);
            }
        }
    }
}
