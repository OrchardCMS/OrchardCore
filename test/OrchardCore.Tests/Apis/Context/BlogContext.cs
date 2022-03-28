using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Controllers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        public const string RemoteDeploymentClientName = "testserver";
        public const string RemoteDeploymentApiKey = "testkey";

        public const string luceneRecipePath = "Areas/TheBlogTheme/Recipes";
        public const string luceneRecipeName = "blog.lucene.recipe.json";

        public string BlogContentItemId { get; private set; }

        public static IShellHost ShellHost { get; }
        private static IShellFeaturesManager ShellFeaturesManager { get; set; }
        private static IEnumerable<IRecipeHarvester> RecipeHarvesters { get; set; }
        private static IRecipeExecutor RecipeExecutor { get; set; }
        private static IEnumerable<IRecipeEnvironmentProvider> EnvironmentProviders { get; set; }
        private static ILogger Logger { get; set; }
        private static IHttpContextAccessor HttpContextAccessor { get; }

        static BlogContext()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
            HttpContextAccessor = Site.Services.GetRequiredService<IHttpContextAccessor>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var shellScope = await ShellHost.GetScopeAsync(TenantName);
            await shellScope.UsingAsync(async scope =>
            {
                ShellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                RecipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();
                RecipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();
                EnvironmentProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeEnvironmentProvider>>();
                Logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminController>>();
                // HttpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                var features = await ShellFeaturesManager.GetAvailableFeaturesAsync();
                var recipes = await GetRecipesAsync(features);

                var recipe = recipes
                    .FirstOrDefault(c => c.RecipeFileInfo.Name == luceneRecipeName && c.BasePath == luceneRecipePath);

                var environment = new Dictionary<string, object>();
                await EnvironmentProviders.OrderBy(x => x.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, Logger);

                var executionId = Guid.NewGuid().ToString("n");

                await RecipeExecutor.ExecuteAsync(executionId, recipe, environment, CancellationToken.None);
            });

            var result = await GraphQLClient
                .Content
                .Query("blog", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });


            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }

        private async Task<IEnumerable<RecipeDescriptor>> GetRecipesAsync(IEnumerable<IFeatureInfo> features)
        {
            var recipeCollections = await Task.WhenAll(RecipeHarvesters.Select(x => x.HarvestRecipesAsync()));
            var recipes = recipeCollections.SelectMany(x => x)
                .Where(r => r.IsSetupRecipe == false &&
                    !r.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase) &&
                    features.Any(f => r.BasePath.Contains(f.Extension.SubPath, StringComparison.OrdinalIgnoreCase)));

            return recipes;
        }

        //private Task PopulateEnvironmentAsync(IDictionary<string, object> environment)
        //{
        //    var feature = mockHttpContextAccessor.Object.HttpContext.Features.Get<RecipeEnvironmentFeature>();
        //    if (feature != null)
        //    {
        //        if (feature.Properties.TryGetValue("AdminUserId", out var adminUserId))
        //        {
        //            environment["AdminUserId"] = adminUserId;
        //        }
        //        if (feature.Properties.TryGetValue("AdminUsername", out var adminUserName))
        //        {
        //            environment["AdminUsername"] = adminUserName;
        //        }
        //        if (feature.Properties.TryGetValue("SiteName", out var siteName))
        //        {
        //            environment["SiteName"] = siteName;
        //        }
        //    }

        //    return Task.CompletedTask;
        //}
    }
}
