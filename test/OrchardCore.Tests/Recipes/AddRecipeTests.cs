using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Recipes;

public class AddRecipeTests
{
    [Fact]
    public void AddRecipe_FromFeatureStartup_RegistersRecipeOfTheFeatureExtension()
    {
        // Arrange
        var services = new FeatureAwareServiceCollection(new ServiceCollection());
        services.SetCurrentFeature(CreateFeature("My.Module.Recipes", "My.Module"));

        // Act
        services.AddRecipe("Recipes/Setup/blog.recipe.json");

        // Assert
        var registration = Assert.Single(GetRegistrations(services));
        Assert.Equal("Recipes/Setup/blog.recipe.json", registration.Path);
        Assert.Equal("My.Module", registration.ExtensionId);
        Assert.Equal("My.Module.Recipes", registration.FeatureId);
    }

    [Fact]
    public void AddRecipe_WithExtension_RegistersRecipeOfThisExtensionForTheCurrentFeature()
    {
        // Arrange
        var services = new FeatureAwareServiceCollection(new ServiceCollection());
        services.SetCurrentFeature(CreateFeature("My.Module.Recipes", "My.Module"));

        // Act
        services.AddRecipe("Recipes/Setup/blog.recipe.json", "My.Theme");

        // Assert
        var registration = Assert.Single(GetRegistrations(services));
        Assert.Equal("My.Theme", registration.ExtensionId);
        Assert.Equal("My.Module.Recipes", registration.FeatureId);
    }

    [Fact]
    public void AddRecipe_OutsideFeatureStartup_RegistersRecipeOfTheContentRoot()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRecipe("Recipes/Host/host.recipe.json");

        // Assert
        var registration = Assert.Single(GetRegistrations(services));
        Assert.Null(registration.ExtensionId);
        Assert.Null(registration.FeatureId);
    }

    [Fact]
    public void AddRecipe_FromSeveralFeatures_KeepsTheFeatureOfEachRecipe()
    {
        // Arrange
        var services = new FeatureAwareServiceCollection(new ServiceCollection());

        // Act
        services.SetCurrentFeature(CreateFeature("Module.A", "Module.A"));
        services.AddRecipe("Recipes/A/a.recipe.json");
        services.SetCurrentFeature(CreateFeature("Module.B", "Module.B"));
        services.AddRecipe("Recipes/B/b.recipe.json");

        // Assert
        var registrations = GetRegistrations(services);
        Assert.Equal(["Module.A", "Module.B"], registrations.Select(registration => registration.FeatureId));
        Assert.Equal(["Module.A", "Module.B"], registrations.Select(registration => registration.ExtensionId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddRecipe_EmptyPath_Throws(string path)
    {
        var services = new ServiceCollection();

        Assert.ThrowsAny<ArgumentException>(() => services.AddRecipe(path));
    }

    [Fact]
    public void SetCurrentFeature_FeatureAwareServiceCollection_ExposesCurrentFeature()
    {
        // Arrange
        var services = new FeatureAwareServiceCollection(new ServiceCollection());
        var feature = CreateFeature("My.Module", "My.Module");

        // Act
        services.SetCurrentFeature(feature);

        // Assert
        Assert.Same(feature, ((IFeatureAwareServiceCollection)services).CurrentFeature);
    }

    private static RecipeRegistration[] GetRegistrations(IServiceCollection services)
        => services.BuildServiceProvider().GetRequiredService<IOptions<RecipeOptions>>().Value.Recipes.ToArray();

    private static IFeatureInfo CreateFeature(string featureId, string extensionId)
    {
        var extension = new Mock<IExtensionInfo>();
        extension.Setup(e => e.Id).Returns(extensionId);

        var feature = new Mock<IFeatureInfo>();
        feature.Setup(f => f.Id).Returns(featureId);
        feature.Setup(f => f.Extension).Returns(extension.Object);

        return feature.Object;
    }
}
