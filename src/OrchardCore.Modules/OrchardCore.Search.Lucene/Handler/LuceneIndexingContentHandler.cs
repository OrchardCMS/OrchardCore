using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentPreview;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Handlers;

public class LuceneIndexingContentHandler : ContentHandlerBase
{
    private readonly List<ContentContextBase> _contexts = [];
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LuceneIndexingContentHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task PublishedAsync(PublishContentContext context) => AddContextAsync(context);
    public override Task CreatedAsync(CreateContentContext context) => AddContextAsync(context);
    public override Task UpdatedAsync(UpdateContentContext context) => AddContextAsync(context);
    public override Task RemovedAsync(RemoveContentContext context) => AddContextAsync(context);
    public override Task UnpublishedAsync(PublishContentContext context) => AddContextAsync(context);

    private Task AddContextAsync(ContentContextBase context)
    {
        // Do not index a preview content item.
        if (_httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing == true)
        {
            return Task.CompletedTask;
        }

        if (context.ContentItem.Id == 0)
        {
            // Ignore that case, when Update is called on a content item which has not be "created" yet.
            return Task.CompletedTask;
        }

        if (_contexts.Count == 0)
        {
            var contexts = _contexts;

            // Using a local var prevents the lambda from holding a ref on this scoped service.
            ShellScope.AddDeferredTask(scope => IndexingAsync(scope, contexts));
        }

        _contexts.Add(context);

        return Task.CompletedTask;
    }

    private static async Task IndexingAsync(ShellScope scope, IEnumerable<ContentContextBase> contexts)
    {
        var services = scope.ServiceProvider;
        var contentManager = services.GetRequiredService<IContentManager>();
        var contentItemIndexHandlers = services.GetServices<IContentItemIndexHandler>();
        var luceneIndexManager = services.GetRequiredService<LuceneIndexManager>();
        var luceneIndexSettingsService = services.GetRequiredService<LuceneIndexSettingsService>();
        var logger = services.GetRequiredService<ILogger<LuceneIndexingContentHandler>>();

        // Multiple items may have been updated in the same scope, e.g through a recipe.
        var contextsGroupById = contexts.GroupBy(c => c.ContentItem.ContentItemId, c => c);

        // Get all contexts for each content item id.
        foreach (var ContextsById in contextsGroupById)
        {
            // Only process the last context.
            var context = ContextsById.Last();

            ContentItem published = null, latest = null;
            bool publishedLoaded = false, latestLoaded = false;

            foreach (var indexSettings in await luceneIndexSettingsService.GetSettingsAsync())
            {
                var cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(context.ContentItem);
                var culture = cultureAspect.HasCulture ? cultureAspect.Culture.Name : null;
                var ignoreIndexedCulture = indexSettings.Culture != "any" && culture != indexSettings.Culture;

                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType) && !ignoreIndexedCulture)
                {
                    if (!indexSettings.IndexLatest && !publishedLoaded)
                    {
                        publishedLoaded = true;
                        published = await contentManager.GetAsync(context.ContentItem.ContentItemId, VersionOptions.Published);
                    }

                    if (indexSettings.IndexLatest && !latestLoaded)
                    {
                        latestLoaded = true;
                        latest = await contentManager.GetAsync(context.ContentItem.ContentItemId, VersionOptions.Latest);
                    }

                    var contentItem = !indexSettings.IndexLatest ? published : latest;

                    if (contentItem == null)
                    {
                        await luceneIndexManager.DeleteDocumentsAsync(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                    }
                    else
                    {
                        var buildIndexContext = new BuildIndexContext(new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId), contentItem, new string[] { contentItem.ContentType }, new LuceneContentIndexSettings());
                        await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                        await luceneIndexManager.DeleteDocumentsAsync(indexSettings.IndexName, new string[] { contentItem.ContentItemId });
                        await luceneIndexManager.StoreDocumentsAsync(indexSettings.IndexName, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                    }
                }
            }
        }
    }
}
