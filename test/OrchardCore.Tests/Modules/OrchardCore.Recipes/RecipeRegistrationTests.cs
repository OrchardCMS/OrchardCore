using Microsoft.Extensions.FileProviders;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Setup.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Recipes;

public class RecipeRegistrationTests
{
    [Fact]
    public async Task HarvestRecipesAsync_RegisteredRecipes_DependOnTheEnabledFeatures()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        var recipes = await HarvestRecipesAsync(context);

        // Assert
        // The Blog recipe enables the Menu and Media features, but not the Admin Dashboard one.
        Assert.Equal("OrchardCore.Menu", Assert.Single(recipes, recipe => recipe.Name == "MenuAddPermissions").FeatureId);
        Assert.Equal("OrchardCore.Media", Assert.Single(recipes, recipe => recipe.Name == "MediaApiPkce").FeatureId);
        Assert.DoesNotContain(recipes, recipe => recipe.Name == "dashboard-widgets-samples");

        // The Default tenant recipes are not available to the other tenants.
        Assert.DoesNotContain(recipes, recipe => recipe.Name == "SaaS");

        // The recipes found in a 'Recipes' folder are still available.
        Assert.Contains(recipes, recipe => recipe.Name == "Blog" && recipe.FeatureId is null);

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = await shellFeaturesManager.GetAvailableFeaturesAsync();

            await shellFeaturesManager.EnableFeaturesAsync(features.Where(feature => feature.Id == "OrchardCore.AdminDashboard"), force: true);
        });

        recipes = await HarvestRecipesAsync(context);

        // Assert
        Assert.Equal("OrchardCore.AdminDashboard", Assert.Single(recipes, recipe => recipe.Name == "dashboard-widgets-samples").FeatureId);
    }

    [Fact]
    public async Task HarvestRecipesAsync_RegisteredRecipe_ResolvesItsFileFromTheModule()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        var recipes = await HarvestRecipesAsync(context);

        // Assert
        var recipe = Assert.Single(recipes, recipe => recipe.Name == "MenuAddPermissions");
        Assert.Equal("Areas/OrchardCore.Menu/Recipes/Permissions", recipe.BasePath);
        Assert.Equal("menu.add-permissions.recipe.json", recipe.RecipeFileInfo.Name);
        Assert.True(recipe.RecipeFileInfo.Exists);
    }

    [Fact]
    public async Task GetSetupRecipesAsync_SaasRecipe_IsOnlyAvailableToTheDefaultTenant()
    {
        // Arrange
        var shellScope = await SiteContext.ShellHost.GetScopeAsync(ShellSettings.DefaultShellName);
        SiteContext.HttpContextAccessor.HttpContext = shellScope.ShellContext.CreateHttpContext();

        IEnumerable<RecipeDescriptor> defaultTenantRecipes = null;
        IEnumerable<RecipeDescriptor> otherTenantRecipes = null;

        // Act
        try
        {
            await shellScope.UsingAsync(async scope =>
            {
                var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();

                using var otherTenantSettings = SiteContext.ShellSettingsManager
                    .CreateDefaultSettings()
                    .AsUninitialized()
                    .AsDisposable();

                otherTenantSettings.Name = "OtherTenant";

                defaultTenantRecipes = await setupService.GetSetupRecipesAsync(scope.ShellContext.Settings);
                otherTenantRecipes = await setupService.GetSetupRecipesAsync(otherTenantSettings);
            }, activateShell: false);
        }
        finally
        {
            SiteContext.HttpContextAccessor.HttpContext = null;
        }

        // Assert
        Assert.Contains(defaultTenantRecipes, recipe => recipe.Name == "SaaS" && recipe.FeatureId == "OrchardCore.Recipes.Default");
        Assert.DoesNotContain(otherTenantRecipes, recipe => recipe.Name == "SaaS");

        // The other setup recipes are available to every tenant.
        Assert.Contains(defaultTenantRecipes, recipe => recipe.Name == "Blog");
        Assert.Contains(otherTenantRecipes, recipe => recipe.Name == "Blog");
        Assert.Contains(otherTenantRecipes, recipe => recipe.Name == "Minimal");
        Assert.All(otherTenantRecipes, recipe => Assert.True(recipe.IsSetupRecipe));
    }

    [Fact]
    public async Task SetupTenant_SaasRecipe_IsRejectedForOtherTenants()
    {
        // Arrange
        var tenantName = Guid.NewGuid().ToString("n");
        var tablePrefix = "t" + tenantName[..8];

        var createResult = await SiteContext.DefaultTenantClient.PostAsJsonAsync("api/tenants/create", new
        {
            Name = tenantName,
            RequestUrlPrefix = tenantName,
            DatabaseProvider = "Sqlite",
            TablePrefix = tablePrefix,
        });

        createResult.EnsureSuccessStatusCode();

        // Act
        var setupResult = await SiteContext.DefaultTenantClient.PostAsJsonAsync("api/tenants/setup", new
        {
            Name = tenantName,
            SiteName = "Test Site",
            DatabaseProvider = "Sqlite",
            TablePrefix = tablePrefix,
            RecipeName = "SaaS",
            UserName = "admin",
            Password = "Password01_",
            Email = "Nick@Orchard",
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, setupResult.StatusCode);
        Assert.Contains("Recipe 'SaaS' not found.", await setupResult.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task RecipesStep_RegisteredRecipe_CanBeExecutedByName()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        Exception exception = null;

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();

            // A recipe whose 'recipes' step runs the registered 'MenuAddPermissions' recipe, the step failing with
            // a 'No recipe named' error if the registered recipe is not found.
            var recipe = new RecipeDescriptor
            {
                Name = "RunRegisteredRecipe",
                FileProvider = new NullFileProvider(),
                BasePath = string.Empty,
                RecipeFileInfo = new InMemoryFileInfo(
                    "run-registered.recipe.json",
                    """{ "name": "RunRegisteredRecipe", "steps": [ { "name": "recipes", "Values": [ { "executionid": "test", "name": "MenuAddPermissions" } ] } ] }"""),
            };

            exception = await Record.ExceptionAsync(() =>
                recipeExecutor.ExecuteAsync(Guid.NewGuid().ToString("n"), recipe, new Dictionary<string, object>(), CancellationToken.None));
        });

        // Assert
        Assert.Null(exception);
    }

    private static async Task<RecipeDescriptor[]> HarvestRecipesAsync(SiteContext context)
    {
        RecipeDescriptor[] recipes = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipeHarvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IRecipeHarvester>>();
            var recipeCollections = await Task.WhenAll(recipeHarvesters.Select(harvester => harvester.HarvestRecipesAsync()));

            recipes = recipeCollections.SelectMany(recipe => recipe).ToArray();
        });

        return recipes;
    }

    private sealed class InMemoryFileInfo : IFileInfo
    {
        private readonly byte[] _content;

        public InMemoryFileInfo(string name, string content)
        {
            Name = name;
            _content = Encoding.UTF8.GetBytes(content);
        }

        public bool Exists => true;

        public long Length => _content.Length;

        public string PhysicalPath => null;

        public string Name { get; }

        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

        public bool IsDirectory => false;

        public Stream CreateReadStream() => new MemoryStream(_content);
    }
}
