using System.Text.Json.Nodes;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Utilities;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdApplicationStepTests
{
    [Theory]
    [ClassData(typeof(OpenIdApplicationStepTestsData))]
    public async Task OpenIdApplicationCanBeParsed(string recipeName, OpenIdApplicationDescriptor expected)
    {
        // Arrange
        OpenIdApplicationDescriptor actual = null;
        var appManagerMock = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);

        appManagerMock.Setup(m =>
            m.FindByClientIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actual);

        appManagerMock.Setup(m =>
            m.CreateAsync(
                It.IsAny<OpenIdApplicationDescriptor>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((app, c) =>
                actual = (OpenIdApplicationDescriptor)app)
            .ReturnsAsync(actual);


        var step = new OpenIdApplicationStep(appManagerMock.Object);
        var recipe = JsonNode.Parse(GetRecipeFileContent(recipeName));
        var context = new RecipeExecutionContext
        {
            Name = recipe["steps"][0].Value<string>("name"),
            Step = (JsonObject)recipe["steps"][0],
        };

        // Act
        await step.ExecuteAsync(context);

        // Assert
        appManagerMock.Verify(m =>
            m.FindByClientIdAsync(
                It.Is<string>(ci => ci == expected.ClientId),
                It.IsAny<CancellationToken>()));

        appManagerMock.Verify(m =>
            m.CreateAsync(
                It.IsAny<OpenIdApplicationDescriptor>(),
                It.IsAny<CancellationToken>()));

        Assert.Equal(expected.ClientId, actual.ClientId);
        Assert.Equal(expected.ClientSecret, actual.ClientSecret);
        Assert.Equal(expected.ClientType, actual.ClientType);
        Assert.Equal(expected.ConsentType, actual.ConsentType);
        Assert.Equal(expected.DisplayName, actual.DisplayName);
        Assert.Equal(expected.Permissions, actual.Permissions);
        Assert.Equal(expected.PostLogoutRedirectUris, actual.PostLogoutRedirectUris);
        Assert.Equal(expected.RedirectUris, actual.RedirectUris);
        Assert.Equal(expected.Roles, actual.Roles);
    }

    [Fact]
    public async Task OpenIdApplicationCanBeUpdated()
    {
        // Arrange
        var recipeName = "app-recipe3";
        var clientId = "a1";
        var expected = new OpenIdApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "Expected Name"
        };
        expected.RedirectUris.UnionWith(new[] { new Uri("https://localhost/redirect") });

        var actual = new OpenIdApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "Actual Name"
        };
        actual.RedirectUris.UnionWith(new[] { new Uri("https://localhost/x") });
        actual.Roles.UnionWith(new[] { "x" });
        actual.Permissions.UnionWith(new[] { $"{OpenIddictConstants.Permissions.Prefixes.Scope}x" });

        var actualDb = new OpenIdApplication
        {
            ClientId = actual.ClientId,
            DisplayName = actual.DisplayName,
            RedirectUris = actual.RedirectUris.Select(u => u.AbsoluteUri).ToImmutableArray(),
            Roles = actual.Roles.ToImmutableArray(),
            Permissions = actual.Permissions.ToImmutableArray()
        };

        var appManagerMock = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);

        appManagerMock.Setup(m =>
            m.FindByClientIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actualDb);

        appManagerMock.Setup(m =>
            m.PopulateAsync(
                It.IsAny<OpenIddictApplicationDescriptor>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(
                ValueTask.CompletedTask);

        appManagerMock.Setup(m =>
            m.UpdateAsync(
                It.IsAny<object>(),
                It.IsAny<OpenIdApplicationDescriptor>(),
                It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((app, desc, c) =>
                actual = (OpenIdApplicationDescriptor)desc)
            .Returns(
                ValueTask.CompletedTask);

        var step = new OpenIdApplicationStep(appManagerMock.Object);
        var recipe = JsonNode.Parse(GetRecipeFileContent(recipeName));
        var context = new RecipeExecutionContext
        {
            Name = recipe["steps"][0].Value<string>("name"),
            Step = (JsonObject)recipe["steps"][0],
        };

        // Act
        await step.ExecuteAsync(context);

        // Assert
        appManagerMock.Verify(m =>
            m.FindByClientIdAsync(
                It.Is<string>(ci => ci == expected.ClientId),
                It.IsAny<CancellationToken>()));

        appManagerMock.Verify(m =>
            m.UpdateAsync(
                It.IsAny<object>(),
                It.IsAny<OpenIdApplicationDescriptor>(),
                It.IsAny<CancellationToken>()));

        Assert.Equal(expected.ClientId, actual.ClientId);
        Assert.Equal(expected.ClientSecret, actual.ClientSecret);
        Assert.Equal(expected.ClientType, actual.ClientType);
        Assert.Equal(expected.ConsentType, actual.ConsentType);
        Assert.Equal(expected.DisplayName, actual.DisplayName);
        Assert.Equal(expected.Permissions, actual.Permissions);
        Assert.Equal(expected.PostLogoutRedirectUris, actual.PostLogoutRedirectUris);
        Assert.Equal(expected.RedirectUris, actual.RedirectUris);
        Assert.Equal(expected.Roles, actual.Roles);
    }

    private string GetRecipeFileContent(string recipeName)
    {
        return new EmbeddedFileProvider(GetType().Assembly)
            .GetFileInfo($"Modules.OrchardCore.OpenId.RecipeFiles.{recipeName}.json")
            .ReadToEnd();
    }
}
