using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Lucene;
using OrchardCore.Testing.Apis;
using OrchardCore.Testing.Apis.Security;
using OrchardCore.Testing.Data;
using OrchardCore.Testing.Infrastructure;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : SiteContextBase<SiteStartup>
    {
        public SiteContext()
        {
            this.WithRecipe("Blog")
                .WithDatabaseProvider("Sqlite");
        }

        public async Task ResetLuceneIndiciesAsync(string indexName)
        {
            var shellScope = await ShellHost.GetScopeAsync(TenantName);

            await shellScope.UsingServiceScopeAsync(async scope =>
            {
                var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);

                luceneIndexingService.ResetIndexAsync(indexName);
                await luceneIndexingService.ProcessContentItemsAsync(indexName);
            });
        }

        public async Task<string> CreateContentItem(string contentType, Action<ContentItem> func, bool draft = false)
        {
            // Never generate a fake ContentItemId here as it should be created by the ContentManager.NewAsync() method.
            // Controllers should use the proper sequence so that they call their event handlers.
            // In that case it would skip calling ActivatingAsync, ActivatedAsync, InitializingAsync, InitializedAsync events
            var contentItem = new ContentItem
            {
                ContentType = contentType
            };

            func(contentItem);

            var content = await Client.PostAsJsonAsync("api/content" + (draft ? "?draft=true" : ""), contentItem);
            var response = await content.Content.ReadAsAsync<ContentItem>();

            return response.ContentItemId;
        }

        public Task DeleteContentItem(string contentItemId) => Client.DeleteAsync("api/content/" + contentItemId);
    }
}
