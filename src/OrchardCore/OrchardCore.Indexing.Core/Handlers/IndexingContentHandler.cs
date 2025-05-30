using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
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
    private readonly Dictionary<string, ContentContextBase> _contexts = [];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexingContentHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task PublishedAsync(PublishContentContext context)
        => AddContextAsync(context);

    public override Task CreatedAsync(CreateContentContext context)
        => AddContextAsync(context);

    public override Task UpdatedAsync(UpdateContentContext context)
        => AddContextAsync(context);

    public override Task RemovedAsync(RemoveContentContext context)
        => AddContextAsync(context);

    public override Task UnpublishedAsync(PublishContentContext context)
        => AddContextAsync(context);

    public override Task ImportCompletedAsync(ImportedContentsContext context)
    {
        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("imported-content-items", async scope =>
        {
            var contentIndexingService = scope.ServiceProvider.GetRequiredService<ContentIndexingService>();

            await contentIndexingService.ProcessContentItemsForAllIndexesAsync();
        });
    }

    private Task AddContextAsync(ContentContextBase context)
    {
        // Do not index a preview content item.
        if (_httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing == true)
        {
            return Task.CompletedTask;
        }

        if (context.ContentItem.Id == 0)
        {
            // Ignore that case, when Update is called on a content item which has not been created yet.
            return Task.CompletedTask;
        }

        if (_contexts.Count == 0)
        {
            var contexts = _contexts;

            // Using a local variable prevents the lambda from holding a ref on this scoped service.
            ShellScope.AddDeferredTask(scope => IndexingAsync(scope, contexts));
        }

        _contexts[context.ContentItem.ContentItemId] = context;

        return Task.CompletedTask;
    }

    private static async Task IndexingAsync(ShellScope scope, Dictionary<string, ContentContextBase> contexts)
    {
        if (contexts.Count == 0)
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

        var documentIndexManagers = new Dictionary<string, IDocumentIndexManager>();

        // Multiple items may have been updated in the same scope, e.g through a recipe.

        // Get all contexts for each content item id.
        foreach (var context in contexts)
        {
            ContentItem contentItem = null;

            foreach (var index in indexes)
            {
                var metadata = index.As<ContentIndexMetadata>();

                var cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(context.Value.ContentItem);
                var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                var ignoreIndexedCulture = metadata.Culture != "any" && culture != metadata.Culture;

                if (ignoreIndexedCulture || !metadata.IndexedContentTypes.Contains(context.Value.ContentItem.ContentType))
                {
                    continue;
                }

                if (metadata.IndexLatest)
                {
                    contentItem = await contentManager.GetAsync(context.Key, VersionOptions.Latest);
                }
                else
                {
                    contentItem = await contentManager.GetAsync(context.Key, VersionOptions.Published);
                }

                if (!documentIndexManagers.TryGetValue(index.ProviderName, out var documentIndexManager))
                {
                    documentIndexManager = services.GetRequiredKeyedService<IDocumentIndexManager>(index.ProviderName);
                    documentIndexManagers.Add(index.ProviderName, documentIndexManager);
                }

                await documentIndexManager.DeleteDocumentsAsync(index, [context.Key]);

                if (contentItem is not null)
                {
                    var document = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);

                    var buildIndexContext = new BuildIndexContext(document, contentItem, [contentItem.ContentType], documentIndexManager.GetContentIndexSettings());

                    await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                    await documentIndexManager.MergeOrUploadDocumentsAsync(index, [buildIndexContext.DocumentIndex]);
                }
            }
        }
    }
}
