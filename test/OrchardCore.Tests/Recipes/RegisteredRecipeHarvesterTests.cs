using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tests.Recipes;

public sealed class RegisteredRecipeHarvesterTests : IDisposable
{
    private const string ModuleId = "My.Module";
    private const string ModuleSubPath = "Areas/My.Module";

    private readonly string _contentRoot;
    private readonly Mock<IExtensionManager> _extensionManager = new();

    public RegisteredRecipeHarvesterTests()
    {
        _contentRoot = Path.Combine(Path.GetTempPath(), "oc-recipes-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(_contentRoot);

        var extension = new Mock<IExtensionInfo>();
        extension.Setup(e => e.Id).Returns(ModuleId);
        extension.Setup(e => e.SubPath).Returns(ModuleSubPath);
        extension.Setup(e => e.Exists).Returns(true);

        var missingExtension = new Mock<IExtensionInfo>();
        missingExtension.Setup(e => e.Exists).Returns(false);

        _extensionManager.Setup(m => m.GetExtension(It.IsAny<string>())).Returns(missingExtension.Object);
        _extensionManager.Setup(m => m.GetExtension(ModuleId)).Returns(extension.Object);
    }

    [Fact]
    public async Task HarvestRecipesAsync_RegisteredRecipe_ReturnsDescriptorOfTheFeature()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog", isSetupRecipe: true);
        var harvester = CreateHarvester(new RecipeRegistration("Recipes/Setup/blog.recipe.json", ModuleId, "My.Module.Recipes"));

        // Act
        var recipes = (await harvester.HarvestRecipesAsync()).ToArray();

        // Assert
        var recipe = Assert.Single(recipes);
        Assert.Equal("Blog", recipe.Name);
        Assert.True(recipe.IsSetupRecipe);
        Assert.Equal("My.Module.Recipes", recipe.FeatureId);
        Assert.Equal($"{ModuleSubPath}/Recipes/Setup", recipe.BasePath);
        Assert.Equal("blog.recipe.json", recipe.RecipeFileInfo.Name);
        Assert.NotNull(recipe.FileProvider);
    }

    [Theory]
    [InlineData("/Recipes/Setup/blog.recipe.json")]
    [InlineData("~/Recipes/Setup/blog.recipe.json")]
    [InlineData("Recipes\\Setup\\blog.recipe.json")]
    public async Task HarvestRecipesAsync_PathWithRootOrBackslashes_IsNormalized(string path)
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(new RecipeRegistration(path, ModuleId, "My.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        var recipe = Assert.Single(recipes);
        Assert.Equal($"{ModuleSubPath}/Recipes/Setup", recipe.BasePath);
    }

    [Fact]
    public async Task HarvestRecipesAsync_NoExtension_ResolvesPathFromContentRoot()
    {
        // Arrange
        WriteRecipe("Recipes/Host/host.recipe.json", "Host");
        var harvester = CreateHarvester(new RecipeRegistration("Recipes/Host/host.recipe.json", extensionId: null, featureId: null));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        var recipe = Assert.Single(recipes);
        Assert.Equal("Host", recipe.Name);
        Assert.Equal("Recipes/Host", recipe.BasePath);
        Assert.Null(recipe.FeatureId);
    }

    [Theory]
    [InlineData("Recipes/blog.recipe.json")]
    [InlineData("recipes/blog.recipe.json")]
    public async Task HarvestRecipesAsync_RecipeInRecipesFolder_IsSkipped(string path)
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(new RecipeRegistration(path, ModuleId, "My.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Empty(recipes);
    }

    [Fact]
    public async Task HarvestRecipesAsync_MissingFile_IsSkipped()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(
            new RecipeRegistration("Recipes/Setup/missing.recipe.json", ModuleId, "My.Module"),
            new RecipeRegistration("Recipes/Setup/blog.recipe.json", ModuleId, "My.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Equal("Blog", Assert.Single(recipes).Name);
    }

    [Fact]
    public async Task HarvestRecipesAsync_UnknownExtension_IsSkipped()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(new RecipeRegistration("Recipes/Setup/blog.recipe.json", "Unknown.Module", "Unknown.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Empty(recipes);
    }

    [Fact]
    public async Task HarvestRecipesAsync_InvalidRecipe_IsSkipped()
    {
        // Arrange
        WriteFile($"{ModuleSubPath}/Recipes/Setup/invalid.recipe.json", "{ not json");
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(
            new RecipeRegistration("Recipes/Setup/invalid.recipe.json", ModuleId, "My.Module"),
            new RecipeRegistration("Recipes/Setup/blog.recipe.json", ModuleId, "My.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Equal("Blog", Assert.Single(recipes).Name);
    }

    [Fact]
    public async Task HarvestRecipesAsync_SameFileRegisteredTwice_ReturnsItOnce()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester(
            new RecipeRegistration("Recipes/Setup/blog.recipe.json", ModuleId, "My.Module.First"),
            new RecipeRegistration("Recipes/Setup/blog.recipe.json", ModuleId, "My.Module.Second"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Equal("My.Module.First", Assert.Single(recipes).FeatureId);
    }

    [Fact]
    public async Task HarvestRecipesAsync_SeveralRecipes_KeepsTheRegistrationOrder()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/b.recipe.json", "B");
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/a.recipe.json", "A");
        var harvester = CreateHarvester(
            new RecipeRegistration("Recipes/Setup/b.recipe.json", ModuleId, "My.Module"),
            new RecipeRegistration("Recipes/Setup/a.recipe.json", ModuleId, "My.Module"));

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Equal(["B", "A"], recipes.Select(recipe => recipe.Name));
    }

    [Fact]
    public async Task HarvestRecipesAsync_NoRegistration_ReturnsNoRecipe()
    {
        // Arrange
        WriteRecipe($"{ModuleSubPath}/Recipes/Setup/blog.recipe.json", "Blog");
        var harvester = CreateHarvester();

        // Act
        var recipes = await harvester.HarvestRecipesAsync();

        // Assert
        Assert.Empty(recipes);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private RegisteredRecipeHarvester CreateHarvester(params RecipeRegistration[] registrations)
    {
        var options = new RecipeOptions();

        foreach (var registration in registrations)
        {
            options.Recipes.Add(registration);
        }

        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.Setup(e => e.ContentRootFileProvider).Returns(new PhysicalFileProvider(_contentRoot));

        return new RegisteredRecipeHarvester(
            Options.Create(options),
            new RecipeReader(NullLogger<RecipeReader>.Instance),
            _extensionManager.Object,
            hostEnvironment.Object,
            NullLogger<RegisteredRecipeHarvester>.Instance);
    }

    private void WriteRecipe(string path, string name, bool isSetupRecipe = false)
        => WriteFile(path, $$"""{ "name": "{{name}}", "displayName": "{{name}}", "issetuprecipe": {{(isSetupRecipe ? "true" : "false")}}, "steps": [] }""");

    private void WriteFile(string path, string content)
    {
        var fullPath = Path.Combine(_contentRoot, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, content);
    }
}
