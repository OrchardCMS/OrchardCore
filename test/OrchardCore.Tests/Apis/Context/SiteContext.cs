using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : IDisposable
    {
        private const string luceneRecipePath = "Areas/TheBlogTheme/Recipes";
        private const string luceneRecipeName = "blog.lucene.recipe.json";

        private static readonly TablePrefixGenerator TablePrefixGenerator = new TablePrefixGenerator();
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static HttpClient DefaultTenantClient { get; }

        public string RecipeName { get; set; } = "Blog";
        public string DatabaseProvider { get; set; } = "Sqlite";
        public string ConnectionString { get; set; }
        public PermissionsContext PermissionsContext { get; set; }

        public HttpClient Client { get; private set; }
        public string TenantName { get; private set; }
        public OrchardGraphQLClient GraphQLClient { get; private set; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            DefaultTenantClient = Site.CreateDefaultClient();
        }

        public virtual async Task InitializeAsync()
        {
            var tenantName = Guid.NewGuid().ToString("n");
            var tablePrefix = await TablePrefixGenerator.GeneratePrefixAsync();

            var createModel = new Tenants.ViewModels.CreateApiViewModel
            {
                DatabaseProvider = DatabaseProvider,
                TablePrefix = tablePrefix,
                ConnectionString = ConnectionString,
                RecipeName = RecipeName,
                Name = tenantName,
                RequestUrlPrefix = tenantName
            };

            var createResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/create", createModel);
            createResult.EnsureSuccessStatusCode();

            var content = await createResult.Content.ReadAsStringAsync();

            var url = new Uri(content.Trim('"'));
            url = new Uri(url.Scheme + "://" + url.Authority + url.LocalPath + "/");

            var setupModel = new Tenants.ViewModels.SetupApiViewModel
            {
                SiteName = "Test Site",
                DatabaseProvider = DatabaseProvider,
                TablePrefix = tablePrefix,
                ConnectionString = ConnectionString,
                RecipeName = RecipeName,
                UserName = "admin",
                Password = "Password01_",
                Name = tenantName,
                Email = "Nick@Orchard"
            };

            var setupResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/setup", setupModel);
            setupResult.EnsureSuccessStatusCode();

            lock (Site)
            {
                Client = Site.CreateDefaultClient(url);
                TenantName = tenantName;
            }

            if (PermissionsContext != null)
            {
                var permissionContextKey = Guid.NewGuid().ToString();
                SiteStartup.PermissionsContexts.TryAdd(permissionContextKey, PermissionsContext);
                Client.DefaultRequestHeaders.Add("PermissionsContext", permissionContextKey);
            }

            GraphQLClient = new OrchardGraphQLClient(Client);
        }

        public async Task RunLuceneRecipe(IShellHost shellHost)
        {
            var shellScope = await shellHost.GetScopeAsync(TenantName);
            await shellScope.UsingAsync(async scope =>
            {
                var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var recipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();

                var recipeEventHandlers = new List<IRecipeEventHandler> { new RecipeEventHandler() };
                var loggerMock = new Mock<ILogger<RecipeExecutor>>();

                var recipeExecutor = new RecipeExecutor(
                    shellHost,
                    scope.ShellContext.Settings,
                    recipeEventHandlers,
                    loggerMock.Object);

                var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
                var recipes = await GetRecipesAsync(features, recipeHarvesters);

                var recipe = recipes
                    .FirstOrDefault(c => c.RecipeFileInfo.Name == luceneRecipeName && c.BasePath == luceneRecipePath);

                var executionId = Guid.NewGuid().ToString("n");

                await recipeExecutor.ExecuteAsync(
                    executionId,
                    recipe,
                    new Dictionary<string, object>(),
                    CancellationToken.None);


            });

            shellScope = await shellHost.GetScopeAsync(TenantName);
            await shellScope.UsingAsync(async scope =>
            {
                var luceneIndexSettingsService = new LuceneIndexSettingsService(
                    scope.ServiceProvider.GetRequiredService<IDocumentManager<LuceneIndexSettingsDocument>>());
                var shellOptions = scope.ServiceProvider.GetRequiredService<IOptions<ShellOptions>>();

                var luceneIndexingService = new LuceneIndexingService(
                    shellHost,
                    scope.ShellContext.Settings,
                    new LuceneIndexingState(
                        shellOptions,
                        scope.ShellContext.Settings),
                    luceneIndexSettingsService,
                    new LuceneIndexManager(
                        scope.ServiceProvider.GetRequiredService<IClock>(),
                        shellOptions,
                        scope.ShellContext.Settings,
                        scope.ServiceProvider.GetRequiredService<ILogger<LuceneIndexManager>>(),
                        new LuceneAnalyzerManager(scope.ServiceProvider.GetRequiredService<IOptions<LuceneOptions>>()),
                        luceneIndexSettingsService),
                    scope.ServiceProvider.GetRequiredService<IIndexingTaskManager>(),
                    scope.ServiceProvider.GetRequiredService<ISiteService>(),
                    scope.ServiceProvider.GetRequiredService<ILogger<LuceneIndexingService>>());


                var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync("Search");

                luceneIndexingService.ResetIndex("Search");
                await luceneIndexingService.ProcessContentItemsAsync("Search");
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

        public Task DeleteContentItem(string contentItemId)
        {
            return Client.DeleteAsync("api/content/" + contentItemId);
        }

        public void Dispose()
        {
            Client?.Dispose();
        }

        private async Task<IEnumerable<RecipeDescriptor>> GetRecipesAsync(
            IEnumerable<IFeatureInfo> features,
            IEnumerable<IRecipeHarvester> recipeHarvesters)
        {
            var recipeCollections = await Task.WhenAll(recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x)
                .Where(r => r.IsSetupRecipe == false &&
                    !r.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase) &&
                    features.Any(f => r.BasePath.Contains(f.Extension.SubPath, StringComparison.OrdinalIgnoreCase)));

            return recipes;
        }

        private class RecipeEventHandler : IRecipeEventHandler
        {
            public RecipeExecutionContext Context { get; private set; }

            public Task RecipeStepExecutedAsync(RecipeExecutionContext context)
            {
                if (String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
                {
                    Context = context;
                }

                return Task.CompletedTask;
            }

            public Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeStepExecutingAsync(RecipeExecutionContext context) => Task.CompletedTask;
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
