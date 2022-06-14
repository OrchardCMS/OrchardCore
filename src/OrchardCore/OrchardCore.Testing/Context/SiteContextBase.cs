using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Testing.Context
{
    public abstract class SiteContextBase : IDisposable
    {
        private static readonly TablePrefixGenerator TablePrefixGenerator = new TablePrefixGenerator();

        public static OrchardTestFixture Site { get; }
        public static HttpClient DefaultTenantClient { get; }

        public string RecipeName { get; set; } = "Blank";
        public string DatabaseProvider { get; set; } = "Sqlite";
        public string ConnectionString { get; set; }
        public PermissionsContext PermissionsContext { get; set; }

        public HttpClient Client { get; private set; }
        public string TenantName { get; private set; }

        public static IShellHost ShellHost { get { return Site.Services.GetRequiredService<IShellHost>(); } }

        static SiteContextBase()
        {
            if (SiteContextConfig.WebStartupClass == null)
            {
                throw new ArgumentNullException(nameof(SiteContextConfig.WebStartupClass));
            }
            SiteStartup.WebStartupClass = SiteContextConfig.WebStartupClass;

            if (SiteContextConfig.Recipies != null)
            {
                SiteStartup.Recipies.AddRange(SiteContextConfig.Recipies);
            }
            if (SiteContextConfig.AdditionalSetupFeatures != null)
            {
                SiteStartup.AdditionalSetupFeatures.AddRange(SiteContextConfig.AdditionalSetupFeatures);
            }
            if (SiteContextConfig.TenantFeatures != null)
            {
                SiteStartup.TenantFeatures.AddRange(SiteContextConfig.TenantFeatures);
            }

            if (SiteContextConfig.ConfigureAppBuilder != null)
            {
                SiteStartup.ConfigureAppBuilder = SiteContextConfig.ConfigureAppBuilder;
            }

            if (SiteContextConfig.ConfigureOrchardServices != null)
            {
                var defaultConfiguration = SiteStartup.ConfigureOrchardServices;
                SiteStartup.ConfigureOrchardServices = new Action<IServiceCollection>(target =>
                {
                    defaultConfiguration.Invoke(target);
                    SiteContextConfig.ConfigureOrchardServices.Invoke(target);
                });
            }

            if (SiteContextConfig.ConfigureHostBuilder != null)
            {
                OrchardTestFixture.ConfigureHostBuilder = SiteContextConfig.ConfigureHostBuilder;
            }

            Site = new OrchardTestFixture();
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
        }

        public async Task RunRecipeAsync(IShellHost shellHost, string recipeName, string recipePath)
        {
            var shellScope = await shellHost.GetScopeAsync(TenantName);
            await shellScope.UsingAsync(async scope =>
            {
                var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var recipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();
                var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                var recipeCollections = await Task.WhenAll(
                    recipeHarvesters.Select(recipe => recipe.HarvestRecipesAsync()));

                var recipes = recipeCollections.SelectMany(recipeCollection => recipeCollection);
                var recipe = recipes
                    .FirstOrDefault(recipe => recipe.RecipeFileInfo.Name == recipeName && recipe.BasePath == recipePath);

                var executionId = Guid.NewGuid().ToString("n");

                await recipeExecutor.ExecuteAsync(
                    executionId,
                    recipe,
                    new Dictionary<string, object>(),
                    CancellationToken.None);
            });
        }

        public void Dispose()
        {
            Client?.Dispose();
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

    }

    public static class SiteContextExtensions
    {
        public static T WithDatabaseProvider<T>(this T siteContext, string databaseProvider) where T : SiteContextBase
        {
            siteContext.DatabaseProvider = databaseProvider;
            return siteContext;
        }

        public static T WithConnectionString<T>(this T siteContext, string connectionString) where T : SiteContextBase
        {
            siteContext.ConnectionString = connectionString;
            return siteContext;
        }

        public static T WithPermissionsContext<T>(this T siteContext, PermissionsContext permissionsContext) where T : SiteContextBase
        {
            siteContext.PermissionsContext = permissionsContext;
            return siteContext;
        }

        public static T WithRecipe<T>(this T siteContext, string recipeName) where T : SiteContextBase
        {
            siteContext.RecipeName = recipeName;
            return siteContext;
        }
    }

}
