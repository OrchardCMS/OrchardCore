using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public class RecipeSchemaServiceTests
{
    [Fact]
    public void GetStepDescriptors_ReturnsEmptyCollection_WhenNoDescriptorsRegistered()
    {
        // Arrange
        var service = new RecipeSchemaService([], []);

        // Act
        var descriptors = service.GetStepDescriptors();

        // Assert
        Assert.Empty(descriptors);
    }

    [Fact]
    public void GetStepDescriptors_ReturnsRegisteredDescriptors()
    {
        // Arrange
        var descriptor1 = new TestRecipeStepDescriptor("Step1", "Step 1", "Description 1");
        var descriptor2 = new TestRecipeStepDescriptor("Step2", "Step 2", "Description 2");
        var service = new RecipeSchemaService([descriptor1, descriptor2], []);

        // Act
        var descriptors = service.GetStepDescriptors().ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, d => d.Name == "Step1");
        Assert.Contains(descriptors, d => d.Name == "Step2");
    }

    [Fact]
    public void GetStepDescriptor_ReturnsCorrectDescriptor_WhenFound()
    {
        // Arrange
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Enables features");
        var service = new RecipeSchemaService([descriptor], []);

        // Act
        var result = service.GetStepDescriptor("Feature");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Feature", result.Name);
        Assert.Equal("Feature Step", result.DisplayName);
    }

    [Fact]
    public void GetStepDescriptor_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var service = new RecipeSchemaService([], []);

        // Act
        var result = service.GetStepDescriptor("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStepDescriptor_IsCaseInsensitive()
    {
        // Arrange
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Enables features");
        var service = new RecipeSchemaService([descriptor], []);

        // Act
        var result = service.GetStepDescriptor("FEATURE");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Feature", result.Name);
    }

    [Fact]
    public async Task GetStepSchemaAsync_ReturnsSchemaFromDescriptor()
    {
        // Arrange
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
            },
        };
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Description", schema);
        var service = new RecipeSchemaService([descriptor], []);

        // Act
        var result = await service.GetStepSchemaAsync("Feature");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("object", result["type"]?.ToString());
    }

    [Fact]
    public async Task GetStepSchemaAsync_ReturnsSchemaFromProvider_WhenAvailable()
    {
        // Arrange
        var descriptorSchema = new JsonObject { ["source"] = "descriptor" };
        var providerSchema = new JsonObject { ["source"] = "provider" };
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Description", descriptorSchema);
        var provider = new TestRecipeStepSchemaProvider("Feature", providerSchema);
        var service = new RecipeSchemaService([descriptor], [provider]);

        // Act
        var result = await service.GetStepSchemaAsync("Feature");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("provider", result["source"]?.ToString());
    }

    [Fact]
    public async Task GetCombinedSchemaAsync_ReturnsValidRecipeSchema()
    {
        // Arrange
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Enables features");
        var service = new RecipeSchemaService([descriptor], []);

        // Act
        var result = await service.GetCombinedSchemaAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", result["$schema"]?.ToString());
        Assert.Equal("Orchard Core Recipe", result["title"]?.ToString());
        Assert.NotNull(result["properties"]);
        Assert.NotNull(result["properties"]["steps"]);
    }

    [Fact]
    public async Task ValidateRecipeAsync_ReturnsSuccess_WhenRecipeIsValid()
    {
        // Arrange
        var descriptor = new TestRecipeStepDescriptor("Feature", "Feature Step", "Enables features");
        var service = new RecipeSchemaService([descriptor], []);
        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["name"] = "Feature",
                    ["enable"] = new JsonArray("OrchardCore.Contents"),
                },
            },
        };

        // Act
        var result = await service.ValidateRecipeAsync(recipe);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateRecipeAsync_ReturnsError_WhenStepsIsMissing()
    {
        // Arrange
        var service = new RecipeSchemaService([], []);
        var recipe = new JsonObject { ["name"] = "TestRecipe" };

        // Act
        var result = await service.ValidateRecipeAsync(recipe);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("steps", result.Errors[0].Message);
    }

    [Fact]
    public async Task ValidateRecipeAsync_ReturnsError_WhenStepHasNoName()
    {
        // Arrange
        var service = new RecipeSchemaService([], []);
        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["enable"] = new JsonArray("OrchardCore.Contents"),
                },
            },
        };

        // Act
        var result = await service.ValidateRecipeAsync(recipe);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("name", result.Errors[0].Message);
    }

    [Fact]
    public async Task ValidateRecipeAsync_ValidatesRequiredProperties()
    {
        // Arrange
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("name", "data"),
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
                ["data"] = new JsonObject { ["type"] = "array" },
            },
        };
        var descriptor = new TestRecipeStepDescriptor("Content", "Content Step", "Imports content", schema);
        var service = new RecipeSchemaService([descriptor], []);
        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["name"] = "Content",
                    // Missing required "data" property
                },
            },
        };

        // Act
        var result = await service.ValidateRecipeAsync(recipe);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("data", result.Errors[0].Message);
    }

    private sealed class TestRecipeStepDescriptor : RecipeStepDescriptor
    {
        private readonly JsonObject _schema;

        public TestRecipeStepDescriptor(string name, string displayName, string description, JsonObject schema = null)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            _schema = schema;
        }

        public override string Name { get; }
        public override string DisplayName { get; }
        public override string Description { get; }

        public override ValueTask<JsonObject> GetSchemaAsync()
            => ValueTask.FromResult(_schema);
    }

    private sealed class TestRecipeStepSchemaProvider : IRecipeStepSchemaProvider
    {
        private readonly JsonObject _schema;

        public TestRecipeStepSchemaProvider(string stepName, JsonObject schema)
        {
            StepName = stepName;
            _schema = schema;
        }

        public string StepName { get; }

        public ValueTask<JsonObject> GetSchemaAsync()
            => ValueTask.FromResult(_schema);
    }
}
