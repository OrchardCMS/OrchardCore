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

    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexingContentHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task PublishedAsync(PublishContentContext context)
        => AddContextAsync(context.ContentItem);

    public override Task CreatedAsync(CreateContentContext context)
        => AddContextAsync(context.ContentItem);

    public override Task UpdatedAsync(UpdateContentContext context)
        => AddContextAsync(context.ContentItem);

    public override Task RemovedAsync(RemoveContentContext context)
        => AddContextAsync(context.ContentItem);

    public override Task UnpublishedAsync(PublishContentContext context)
        => AddContextAsync(context.ContentItem);

    public override Task ImportCompletedAsync(ImportedContentsContext context)
    {
        var contentItems = context.ImportedItems;

        ShellScope.AddDeferredTask(scope => IndexAsync(scope, contentItems));

        return Task.CompletedTask;
    }

    private Task AddContextAsync(ContentItem contentItem)
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

        if (_contentItems.Count == 0)
        {
            var contentItems = _contentItems;

            // Using a local variable prevents the lambda from holding a ref on this scoped service.
            ShellScope.AddDeferredTask(scope => IndexAsync(scope, contentItems.Values));
        }

        // Store the last content item in the request.
        _contentItems[contentItem.ContentItemId] = contentItem;

        return Task.CompletedTask;
    }

    private static async Task IndexAsync(ShellScope scope, IEnumerable<ContentItem> contentItems)
    {
        if (!contentItems.Any())
        {
            return;
        }

        var services = scope.ServiceProvider;

        var indexStore = services.GetRequiredService<IIndexEntityStore>();

        var indexes = await indexStore.GetAllAsync();

        if (!indexes.Any())
        {
            return;
        }

        var contentManager = services.GetRequiredService<IContentManager>();
        var contentItemIndexHandlers = services.GetServices<IContentItemIndexHandler>();

        var logger = services.GetRequiredService<ILogger<IndexingContentHandler>>();

        // Multiple items may have been updated in the same scope, e.g through a recipe.
        foreach (var index in indexes)
        {
            var metadata = index.As<ContentIndexMetadata>();
            var documentIndexManager = services.GetKeyedService<IDocumentIndexManager>(index.ProviderName);
            var contentTypes = metadata.IndexedContentTypes.ToHashSet();
            if (documentIndexManager == null)
            {
                logger.LogWarning("No document index manager found for provider '{ProviderName}'.", index.ProviderName);

                continue;
            }

            foreach (var contentItem in contentItems)
            {
                if (!contentTypes.Contains(contentItem.ContentType))
                {
                    continue;
                }

                await documentIndexManager.DeleteDocumentsAsync(index, [contentItem.ContentItemId]);

                var cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(contentItem);
                var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                var ignoreIndexedCulture = metadata.Culture != "any" && culture != metadata.Culture;

                if (ignoreIndexedCulture)
                {
                    continue;
                }

                if (metadata.IndexLatest && !contentItem.Latest)
                {
                    continue;
                }

                if (!metadata.IndexLatest && !contentItem.Published)
                {
                    continue;
                }

                var document = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);

                var buildIndexContext = new BuildIndexContext(document, contentItem, [contentItem.ContentType], documentIndexManager.GetContentIndexSettings());

                await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                await documentIndexManager.MergeOrUploadDocumentsAsync(index, [buildIndexContext.DocumentIndex]);
            }
        }

    }
}
