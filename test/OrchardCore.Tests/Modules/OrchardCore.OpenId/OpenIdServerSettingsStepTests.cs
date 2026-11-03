using System.Text.Json.Nodes;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdServerSettingsStepTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task OpenIdServerSettingsStep_SetsRequireEndSessionConfirmation(bool? stepValue, bool expected)
    {
        // Arrange
        var settings = new OpenIdServerSettings
        {
            // Start from the opposite value to ensure the step always writes the flag.
            RequireEndSessionConfirmation = !expected,
        };

        OpenIdServerSettings updatedSettings = null;

        var openIdServerService = new Mock<IOpenIdServerService>();
        openIdServerService
            .Setup(m => m.LoadSettingsAsync())
            .ReturnsAsync(settings);

        openIdServerService
            .Setup(m => m.UpdateSettingsAsync(It.IsAny<OpenIdServerSettings>()))
            .Callback<OpenIdServerSettings>(value => updatedSettings = value)
            .Returns(Task.CompletedTask);

        var step = new JsonObject
        {
            ["name"] = nameof(OpenIdServerSettings),
        };

        if (stepValue is not null)
        {
            step[nameof(OpenIdServerSettings.RequireEndSessionConfirmation)] = stepValue.Value;
        }

        var recipeContext = new RecipeExecutionContext
        {
            RecipeDescriptor = new RecipeDescriptor(),
            Name = nameof(OpenIdServerSettings),
            Step = step,
        };

        // Act
        await new OpenIdServerSettingsStep(openIdServerService.Object).ExecuteAsync(recipeContext);

        // Assert
        Assert.NotNull(updatedSettings);
        Assert.Equal(expected, updatedSettings.RequireEndSessionConfirmation);
    }
}
