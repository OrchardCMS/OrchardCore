using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentPreview;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core.Handlers;

public class IndexingContentHandler : ContentHandlerBase
{
    private readonly Dictionary<string, ContentItem> _contentItems = [];

    private readonly Dictionary<string, ContentItem> _removedContentItems = [];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexingContentHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task PublishedAsync(PublishContentContext context)
        => AddContentItemAsync(context.ContentItem);

    public override Task CreatedAsync(CreateContentContext context)
        => AddContentItemAsync(context.ContentItem);

    public override Task UpdatedAsync(UpdateContentContext context)
        => AddContentItemAsync(context.ContentItem);

    public override Task RemovedAsync(RemoveContentContext context)
        => RemovedContentItemAsync(context.ContentItem);

    public override Task DraftSavedAsync(SaveDraftContentContext context)
        => AddContentItemAsync(context.ContentItem);

    public override Task UnpublishedAsync(PublishContentContext context)
        => AddContentItemAsync(context.ContentItem);

    private Task RemovedContentItemAsync(ContentItem contentItem)
    {
        if (contentItem.Id == 0)
        {
            // Ignore that case, when Update is called on a content item which has not been created yet.
            return Task.CompletedTask;
        }

        AddDeferredTask();

        // Store the last content item in the request.
        _removedContentItems[contentItem.ContentItemId] = contentItem;

        return Task.CompletedTask;
    }

    private Task AddContentItemAsync(ContentItem contentItem)
    {
        // Do not index a preview content item.
        if (_httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing == true)
        {
            return Task.CompletedTask;
        }

        if (contentItem.Id == 0)
        {
            // Ignore that case, when Update is called on a content item which has not been created yet.
            return Task.CompletedTask;
        }

        AddDeferredTask();

        // Store the last content item in the request.
        _contentItems[contentItem.ContentItemId] = contentItem;

        return Task.CompletedTask;
    }

    private bool _taskAdded;

    private void AddDeferredTask()
    {
        if (_taskAdded)
        {
            return;
        }

        _taskAdded = true;

        // Using a local variable prevents the lambda from holding a ref on this scoped service.
        var contentItems = _contentItems;
        var removedContentItems = _removedContentItems;

        ShellScope.AddDeferredTask(scope => IndexAsync(scope, contentItems.Values, removedContentItems.Keys));
    }

    private static async Task IndexAsync(ShellScope scope, IEnumerable<ContentItem> contentItems, IEnumerable<string> removedIds)
    {
        if (!contentItems.Any() && !removedIds.Any())
        {
            return;
        }

        var services = scope.ServiceProvider;

        var indexStore = services.GetRequiredService<IIndexProfileStore>();

        var indexProfiles = await indexStore.GetByTypeAsync(IndexingConstants.ContentsIndexSource);

        if (!indexProfiles.Any())
        {
            return;
        }

        var contentManager = services.GetRequiredService<IContentManager>();
        var contentItemIndexHandlers = services.GetServices<IDocumentIndexHandler>();

        var logger = services.GetRequiredService<ILogger<IndexingContentHandler>>();

        var allContentItems = contentItems.Select(x => x.ContentItemId).ToArray();

        // Multiple items may have been updated in the same scope, e.g through a recipe.

        var cultureAspects = new Dictionary<string, CultureAspect>();

        foreach (var indexProfile in indexProfiles)
        {
            var documentIndexManager = services.GetKeyedService<IDocumentIndexManager>(indexProfile.ProviderName);
            if (documentIndexManager == null)
            {
                logger.LogWarning("No document index manager found for provider '{ProviderName}'.", indexProfile.ProviderName);

                continue;
            }

            var metadata = indexProfile.As<ContentIndexMetadata>();
            var contentTypes = metadata.IndexedContentTypes.ToHashSet();
            var documents = new List<ContentItemDocumentIndex>();
            var anyCulture = string.IsNullOrEmpty(metadata.Culture) || metadata.Culture == "any";

            foreach (var contentItem in contentItems)
            {
                if (!contentTypes.Contains(contentItem.ContentType))
                {
                    continue;
                }

                if (!anyCulture)
                {
                    if (!cultureAspects.TryGetValue(contentItem.ContentItemVersionId ?? contentItem.ContentItemId, out var cultureAspect))
                    {
                        cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(contentItem);
                        cultureAspects[contentItem.ContentItemVersionId ?? contentItem.ContentItemId] = cultureAspect;
                    }

                    if (cultureAspect.Culture?.Name != metadata.Culture)
                    {
                        continue;
                    }
                }

                if (metadata.IndexLatest && !contentItem.Latest)
                {
                    continue;
                }

                if (!metadata.IndexLatest && !contentItem.Published)
                {
                    continue;
                }

                var document = new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);

                var buildIndexContext = new BuildDocumentIndexContext(document, contentItem, [contentItem.ContentType], documentIndexManager.GetContentIndexSettings());

                await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                documents.Add(document);
            }

            // Delete all of the documents that we'll be updating in this scope.
            await documentIndexManager.DeleteDocumentsAsync(indexProfile, contentItems.Select(x => x.ContentItemId));

            if (documents.Count > 0)
            {
                // Update all of the documents that were updated in this scope.
                await documentIndexManager.AddOrUpdateDocumentsAsync(indexProfile, documents);
            }

            // At the end of the indexing, we remove the documents that were removed in the same scope.
            await documentIndexManager.DeleteDocumentsAsync(indexProfile, removedIds);
        }
    }
}
