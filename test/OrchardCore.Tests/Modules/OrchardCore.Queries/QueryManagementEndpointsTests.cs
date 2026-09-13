using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Localization;
using global::OrchardCore.Queries;
using OrchardCore.Queries.Endpoints.Api;
using OrchardCore.Queries.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Queries;

public class QueryManagementEndpointsTests
{
    [Fact]
    public async Task CreateAsync_ExactRetryReturnsExistingQueryWithoutCreatingDuplicate()
    {
        var queryManager = new Mock<IQueryManager>();
        queryManager.Setup(manager => manager.GetQueryAsync("PublishedContent")).ReturnsAsync(CreateQuery());
        var definition = CreateDefinition();

        var result = await QueryManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            [CreateQuerySource()],
            [],
            CreateLocalizer(),
            definition);

        Assert.Equal(StatusCodes.Status201Created, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        queryManager.Verify(manager => manager.NewAsync(It.IsAny<string>(), It.IsAny<System.Text.Json.Nodes.JsonNode>()), Times.Never);
        queryManager.Verify(manager => manager.SaveAsync(It.IsAny<Query[]>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ConflictingRetryReturnsConflict()
    {
        var queryManager = new Mock<IQueryManager>();
        queryManager.Setup(manager => manager.GetQueryAsync("PublishedContent")).ReturnsAsync(CreateQuery());
        var definition = CreateDefinition();
        definition.ReturnContentItems = false;

        var result = await QueryManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            [CreateQuerySource()],
            [],
            CreateLocalizer(),
            definition);

        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        queryManager.Verify(manager => manager.NewAsync(It.IsAny<string>(), It.IsAny<System.Text.Json.Nodes.JsonNode>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RouteBodyNameMismatchIsRejectedWithoutMutation()
    {
        var queryManager = new Mock<IQueryManager>();
        var definition = CreateDefinition();
        definition.Name = "RenamedQuery";

        var result = await QueryManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            [CreateQuerySource()],
            [],
            CreateLocalizer(),
            "PublishedContent",
            definition);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        queryManager.Verify(manager => manager.UpdateAsync(It.IsAny<Query>(), It.IsAny<System.Text.Json.Nodes.JsonNode>()), Times.Never);
        queryManager.Verify(manager => manager.DeleteQueryAsync(It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SameNameRetryPreservesOmittedProperties()
    {
        var query = CreateQuery();
        var queryManager = new Mock<IQueryManager>();
        queryManager.Setup(manager => manager.GetQueryAsync("PublishedContent")).ReturnsAsync(query);
        System.Text.Json.Nodes.JsonNode updatedData = null;
        queryManager.Setup(manager => manager.UpdateAsync(query, It.IsAny<System.Text.Json.Nodes.JsonNode>()))
            .Callback<Query, System.Text.Json.Nodes.JsonNode>((_, data) => updatedData = data)
            .Returns(Task.CompletedTask);
        var definition = new QueryDefinitionDto
        {
            Name = "PublishedContent",
        };

        var first = await QueryManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            [CreateQuerySource()],
            [],
            CreateLocalizer(),
            "PublishedContent",
            definition);
        var retry = await QueryManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            [CreateQuerySource()],
            [],
            CreateLocalizer(),
            "PublishedContent",
            definition);

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(first).StatusCode);
        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(retry).StatusCode);
        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(query.Properties, updatedData[nameof(Query.Properties)]));
        queryManager.Verify(manager => manager.UpdateAsync(query, It.IsAny<System.Text.Json.Nodes.JsonNode>()), Times.Exactly(2));
        queryManager.Verify(manager => manager.DeleteQueryAsync(It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_MissingQueryReturnsNoContent()
    {
        var queryManager = new Mock<IQueryManager>();
        queryManager.Setup(manager => manager.GetQueryAsync("missing")).ReturnsAsync((Query)null);

        var result = await QueryManagementEndpoints.DeleteAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            queryManager.Object,
            "missing");

        Assert.Equal(StatusCodes.Status204NoContent, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        queryManager.Verify(manager => manager.DeleteQueryAsync(It.IsAny<string[]>()), Times.Never);
    }

    private static Query CreateQuery()
        => new()
        {
            Name = "PublishedContent",
            Source = "Sql",
            ReturnContentItems = true,
            Properties = new System.Text.Json.Nodes.JsonObject
            {
                ["query"] = "select * from ContentItemIndex",
            },
        };

    private static QueryDefinitionDto CreateDefinition()
        => new()
        {
            Name = " PublishedContent ",
            Source = "sql",
            ReturnContentItems = true,
            Properties = new System.Text.Json.Nodes.JsonObject
            {
                ["query"] = "select * from ContentItemIndex",
            },
        };

    private static IQuerySource CreateQuerySource()
    {
        var querySource = new Mock<IQuerySource>();
        querySource.SetupGet(source => source.Name).Returns("Sql");
        return querySource.Object;
    }

    private static IAuthorizationService CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService.Object;
    }

    private static IStringLocalizer<global::OrchardCore.Queries.Startup> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<global::OrchardCore.Queries.Startup>>();
        localizer.Setup(value => value[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        return localizer.Object;
    }
}
