using System.Text.Json.Nodes;
using Json.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public class RecipeSchemaServiceTests
{
    [Fact]
    public void GetSteps_ReturnsEmptyCollection_WhenNoStepsRegistered()
    {
        // Arrange
        var service = new RecipeSchemaService([]);

        // Act
        var steps = service.GetSteps();

        // Assert
        Assert.Empty(steps);
    }

    [Fact]
    public void GetSteps_ReturnsRegisteredSteps()
    {
        // Arrange
        var step1 = new TestRecipeDeploymentStep("Step1");
        var step2 = new TestRecipeDeploymentStep("Step2");
        var service = new RecipeSchemaService([step1, step2]);

        // Act
        var steps = service.GetSteps().ToList();

        // Assert
        Assert.Equal(2, steps.Count);
        Assert.Contains(steps, s => s.Name == "Step1");
        Assert.Contains(steps, s => s.Name == "Step2");
    }

    [Fact]
    public void GetStep_ReturnsCorrectStep_WhenFound()
    {
        // Arrange
        var step = new TestRecipeDeploymentStep("Feature");
        var service = new RecipeSchemaService([step]);

        // Act
        var result = service.GetStep("Feature");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Feature", result.Name);
    }

    [Fact]
    public void GetStep_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var service = new RecipeSchemaService([]);

        // Act
        var result = service.GetStep("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStep_IsCaseInsensitive()
    {
        // Arrange
        var step = new TestRecipeDeploymentStep("Feature");
        var service = new RecipeSchemaService([step]);

        // Act
        var result = service.GetStep("FEATURE");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Feature", result.Name);
    }

    [Fact]
    public void GetStepSchema_ReturnsSchemaFromStep()
    {
        // Arrange
        var schema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(("name", new JsonSchemaBuilder().Type(SchemaValueType.String)))
            .Build();
        var step = new TestRecipeDeploymentStep("Feature", schema);
        var service = new RecipeSchemaService([step]);

        // Act
        var result = service.GetStepSchema("Feature");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetCombinedSchema_ReturnsValidRecipeSchema()
    {
        // Arrange
        var step = new TestRecipeDeploymentStep("Feature");
        var service = new RecipeSchemaService([step]);

        // Act
        var result = service.GetRecipeSchema();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Orchard Core Recipe", result.GetTitle());
        var properties = result.GetProperties();
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("steps"));
    }

    [Fact]
    public void ValidateRecipe_ReturnsSuccess_WhenRecipeIsValid()
    {
        // Arrange
        var step = new TestRecipeDeploymentStep("Feature");
        var service = new RecipeSchemaService([step]);
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
        var result = service.ValidateRecipe(recipe);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateRecipe_ReturnsError_WhenStepsIsMissing()
    {
        // Arrange
        var service = new RecipeSchemaService([]);
        var recipe = new JsonObject { ["name"] = "TestRecipe" };

        // Act
        var result = service.ValidateRecipe(recipe);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateRecipe_ReturnsError_WhenStepHasNoName()
    {
        // Arrange
        var service = new RecipeSchemaService([]);
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
        var result = service.ValidateRecipe(recipe);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    private sealed class TestRecipeDeploymentStep : IRecipeDeploymentStep
    {
        private readonly JsonSchema _schema;

        public TestRecipeDeploymentStep(string name, JsonSchema schema = null)
        {
            Name = name;
            _schema = schema ?? new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Required("name")
                .Properties(("name", new JsonSchemaBuilder().Type(SchemaValueType.String).Const(name)))
                .AdditionalProperties(JsonSchema.Empty)
                .Build();
        }

        public string Name { get; }

        public JsonSchema Schema => _schema;

        public Task ExecuteAsync(RecipeExecutionContext context) => Task.CompletedTask;

        public Task ExportAsync(RecipeExportContext context) => Task.CompletedTask;
    }
}
