using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Recipes.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Recipes;
using OrchardCore.Secrets.Services;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretsRecipeStepTests
{
    private readonly Mock<ISecretManager> _secretManagerMock;
    private readonly Mock<ISecretEncryptionService> _encryptionServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<SecretsRecipeStep>> _loggerMock;
    private readonly Mock<IStringLocalizer<SecretsRecipeStep>> _localizerMock;

    public SecretsRecipeStepTests()
    {
        _secretManagerMock = new Mock<ISecretManager>();
        _encryptionServiceMock = new Mock<ISecretEncryptionService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<SecretsRecipeStep>>();
        _localizerMock = new Mock<IStringLocalizer<SecretsRecipeStep>>();
    }

    private SecretsRecipeStep CreateStep()
    {
        return new SecretsRecipeStep(
            _secretManagerMock.Object,
            _encryptionServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ImportsSecretWithValueFromRecipe()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "TestSecret",
                    ["Value"] = "secret-value-from-recipe",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",  // Required for NamedRecipeStepHandler
            Step = recipeStep,
        };

        ISecret capturedSecret = null;
        _secretManagerMock
            .Setup(m => m.SaveSecretAsync(
                It.IsAny<string>(),
                It.IsAny<ISecret>()))
            .Callback<string, ISecret>((name, secret) => capturedSecret = secret)
            .Returns(Task.CompletedTask);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.Is<string>(s => s == "TestSecret"), It.IsAny<ISecret>()),
            Times.Once);
        Assert.NotNull(capturedSecret);
        var textSecret = Assert.IsType<TextSecret>(capturedSecret);
        Assert.Equal("secret-value-from-recipe", textSecret.Text);
    }

    [Fact]
    public async Task ExecuteAsync_ImportsSecretWithValueFromEnvironmentVariable()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "EnvSecret",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        // Setup configuration to return value from environment variable pattern
        _configurationMock
            .Setup(c => c["OrchardCore_Secrets__EnvSecret"])
            .Returns("env-secret-value");

        ISecret capturedSecret = null;
        _secretManagerMock
            .Setup(m => m.SaveSecretAsync(
                It.IsAny<string>(),
                It.IsAny<ISecret>()))
            .Callback<string, ISecret>((name, secret) => capturedSecret = secret)
            .Returns(Task.CompletedTask);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.Is<string>(s => s == "EnvSecret"), It.IsAny<ISecret>()),
            Times.Once);
        Assert.NotNull(capturedSecret);
        var textSecret = Assert.IsType<TextSecret>(capturedSecret);
        Assert.Equal("env-secret-value", textSecret.Text);
    }

    [Fact]
    public async Task ExecuteAsync_ImportsSecretWithValueFromColonSeparatedConfig()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "ColonSecret",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        // First pattern returns null, second pattern returns value
        _configurationMock
            .Setup(c => c["OrchardCore_Secrets__ColonSecret"])
            .Returns((string)null);
        _configurationMock
            .Setup(c => c["OrchardCore:Secrets:ColonSecret"])
            .Returns("colon-secret-value");

        ISecret capturedSecret = null;
        _secretManagerMock
            .Setup(m => m.SaveSecretAsync(
                It.IsAny<string>(),
                It.IsAny<ISecret>()))
            .Callback<string, ISecret>((name, secret) => capturedSecret = secret)
            .Returns(Task.CompletedTask);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        Assert.NotNull(capturedSecret);
        var textSecret = Assert.IsType<TextSecret>(capturedSecret);
        Assert.Equal("colon-secret-value", textSecret.Text);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsSecret_WhenNoValueProvided()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "NoValueSecret",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        // Configuration returns null for all patterns
        _configurationMock
            .Setup(c => c[It.IsAny<string>()])
            .Returns((string)null);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_AddsError_WhenSecretNameIsMissing()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Value"] = "some-value",
                    // Name is missing
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        // Act
        await step.ExecuteAsync(context);

        // Assert - the error message uses the localizer S["..."]
        Assert.NotEmpty(context.Errors);
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SavesToSpecificStore_WhenStoreSpecified()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "StoreSpecificSecret",
                    ["Store"] = "AzureKeyVault",
                    ["Value"] = "azure-secret-value",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        _secretManagerMock
            .Setup(m => m.SaveSecretAsync(
                It.IsAny<string>(),
                It.IsAny<ISecret>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(
                It.Is<string>(s => s == "StoreSpecificSecret"),
                It.IsAny<ISecret>(),
                It.Is<string>(s => s == "AzureKeyVault")),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ImportsMultipleSecrets()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = new JsonArray
            {
                new JsonObject
                {
                    ["Name"] = "Secret1",
                    ["Value"] = "value1",
                },
                new JsonObject
                {
                    ["Name"] = "Secret2",
                    ["Value"] = "value2",
                },
                new JsonObject
                {
                    ["Name"] = "Secret3",
                    ["Value"] = "value3",
                },
            },
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        _secretManagerMock
            .Setup(m => m.SaveSecretAsync(
                It.IsAny<string>(),
                It.IsAny<ISecret>()))
            .Returns(Task.CompletedTask);

        // Act
        await step.ExecuteAsync(context);

        // Assert
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNullSecretsArray()
    {
        // Arrange
        var step = CreateStep();

        var recipeStep = new JsonObject
        {
            ["name"] = "Secrets",
            // Secrets array is missing
        };

        var context = new RecipeExecutionContext
        {
            Name = "Secrets",
            Step = recipeStep,
        };

        // Act
        await step.ExecuteAsync(context);

        // Assert - should not throw
        _secretManagerMock.Verify(
            m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>()),
            Times.Never);
    }
}
