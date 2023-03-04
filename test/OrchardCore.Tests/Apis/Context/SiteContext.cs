using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Search.Lucene;
using OrchardCore.Testing.Apis;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : SiteContextBase<SiteStartup>
    {
        public SiteContext()
        {
            this.WithRecipe("Blog")
                .WithDatabaseProvider("Sqlite");
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            GraphQLClient = new OrchardGraphQLClient(Client);
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

        public async Task DeleteContentItem(string contentItemId) => await Client.DeleteAsync("api/content/" + contentItemId);
    }
}
