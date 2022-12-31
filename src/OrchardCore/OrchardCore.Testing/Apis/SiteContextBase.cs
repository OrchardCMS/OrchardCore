using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.ViewModels;
using OrchardCore.Testing.Data;
using OrchardCore.Testing.Infrastructure;

namespace OrchardCore.Testing.Apis
{
    public abstract class SiteContextBase<TSiteStartup> : ISiteContext<TSiteStartup> where TSiteStartup : class
    {
        static SiteContextBase()
        {
            Site = new OrchardCoreTestFixture<TSiteStartup>();
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
            DefaultTenantClient = Site.CreateDefaultClient();
        }

        public SiteContextBase()
        {
            Options = new SiteContextOptions();
        }

        public static OrchardCoreTestFixture<TSiteStartup> Site { get; }

        public static IShellHost ShellHost { get; private set; }

        public static HttpClient DefaultTenantClient { get; }

        public SiteContextOptions Options { init; get; }

        public HttpClient Client { get; private set; }

        public string TenantName { get; private set; }

        public OrchardGraphQLClient GraphQLClient { get; protected set; }

        public virtual async Task InitializeAsync()
        {
            var tenantName = Guid.NewGuid().ToString("n");

            if (String.IsNullOrEmpty(Options.TablePrefix))
            {
                Options.TablePrefix = await new TablePrefixGenerator().GeneratePrefixAsync();
            }

            var createResult = await CreateSiteAsync(tenantName);

            var content = await createResult.Content.ReadAsStringAsync();

            await SetupSiteAsync(tenantName);

            lock (Site)
            {
                var url = new Uri(content.Trim('"'));
                url = new Uri(url.Scheme + "://" + url.Authority + url.LocalPath + "/");

                Client = Site.CreateDefaultClient(url);

                TenantName = tenantName;
            }

            if (Options.PermissionsContext != null)
            {
                var permissionContextKey = Guid.NewGuid().ToString();

                SiteContextOptions.PermissionsContexts.TryAdd(permissionContextKey, Options.PermissionsContext);

                Client.DefaultRequestHeaders.Add("PermissionsContext", permissionContextKey);
            }

            GraphQLClient = new OrchardGraphQLClient(Client);
        }

        public async Task RunRecipeAsync(string recipeName, string recipePath)
        {
            var shellScope = await ShellHost.GetScopeAsync(TenantName);

            await shellScope.UsingServiceScopeAsync(async scope =>
            {
                var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var recipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();
                var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                var recipeCollections = await Task.WhenAll(recipeHarvesters
                    .Select(recipe => recipe.HarvestRecipesAsync()));

                var recipe = recipeCollections
                    .SelectMany(r => r)
                    .FirstOrDefault(d => d.RecipeFileInfo.Name == recipeName && d.BasePath == recipePath);

                var executionId = Guid.NewGuid().ToString("n");

                await recipeExecutor.ExecuteAsync(executionId, recipe, new Dictionary<string, object>(), CancellationToken.None);
            });
        }

        public void Dispose() => Client?.Dispose();

        private async Task<HttpResponseMessage> CreateSiteAsync(string tenantName)
        {
            var createModel = new CreateApiViewModel
            {
                DatabaseProvider = Options.DatabaseProvider,
                TablePrefix = Options.TablePrefix,
                ConnectionString = Options.ConnectionString,
                RecipeName = Options.RecipeName,
                Name = tenantName,
                RequestUrlPrefix = tenantName
            };

            var result = await DefaultTenantClient.PostAsJsonAsync("api/tenants/create", createModel);

            result.EnsureSuccessStatusCode();

            return result;
        }

        private async Task SetupSiteAsync(string tenantName)
        {
            var setupModel = new SetupApiViewModel
            {
                SiteName = "Orchard Core Site",
                DatabaseProvider = Options.DatabaseProvider,
                TablePrefix = Options.TablePrefix,
                ConnectionString = Options.ConnectionString,
                RecipeName = Options.RecipeName,
                UserName = "admin",
                Password = "P@ssw0rd",
                Name = tenantName,
                Email = "admin@OrchardCore.net"
            };

            var result = await DefaultTenantClient.PostAsJsonAsync("api/tenants/setup", setupModel);

            result.EnsureSuccessStatusCode();
        }
    }
}
