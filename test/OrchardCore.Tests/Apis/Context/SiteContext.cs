using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene;
using OrchardCore.Recipes.Services;
using OrchardCore.Testing.Context;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : SiteContext<SiteStartup>
    {
        public OrchardGraphQLClient GraphQLClient { get; private set; }

        public SiteContext()
        {
            RecipeName = "Blog";
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            GraphQLClient = new OrchardGraphQLClient(Client);
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

        public Task DeleteContentItem(string contentItemId)
        {
            return Client.DeleteAsync("api/content/" + contentItemId);
        }

        public async Task ResetLuceneIndiciesAsync(IShellHost shellHost, string indexName)
        {
            var shellScope = await shellHost.GetScopeAsync(TenantName);
            await shellScope.UsingAsync(async scope =>
            {
                var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);

                luceneIndexingService.ResetIndex(indexName);
                await luceneIndexingService.ProcessContentItemsAsync(indexName);
            });
        }

    }

    public static class SiteContextExtensions
    {
        public static T WithDatabaseProvider<T>(this T siteContext, string databaseProvider) where T : SiteContext
        {
            siteContext.DatabaseProvider = databaseProvider;
            return siteContext;
        }

        public static T WithConnectionString<T>(this T siteContext, string connectionString) where T : SiteContext
        {
            siteContext.ConnectionString = connectionString;
            return siteContext;
        }

        public static T WithPermissionsContext<T>(this T siteContext, PermissionsContext permissionsContext) where T : SiteContext
        {
            siteContext.PermissionsContext = permissionsContext;
            return siteContext;
        }

        public static T WithRecipe<T>(this T siteContext, string recipeName) where T : SiteContext
        {
            siteContext.RecipeName = recipeName;
            return siteContext;
        }
    }
}
