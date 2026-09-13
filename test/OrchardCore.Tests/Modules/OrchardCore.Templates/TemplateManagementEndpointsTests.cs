using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Templates.Endpoints.Management;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public class TemplateManagementEndpointsTests
{
    [Fact]
    public async Task CreateAsync_ExactRetry_ReturnsExistingDefinitionWithoutMutation()
    {
        var document = CreateDocument();
        var templatesManager = CreateManager(document, out var documentManager);

        var result = await TemplateManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            CreateDefinition("content__article"));

        var created = Assert.IsType<Created<TemplateDefinitionDto>>(result);
        Assert.Equal("Content__Article", created.Value.Name);
        documentManager.Verify(
            manager => manager.UpdateAsync(It.IsAny<TemplatesDocument>(), It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DifferentDefinitionForExistingName_ReturnsConflictWithoutMutation()
    {
        var document = CreateDocument();
        var templatesManager = CreateManager(document, out var documentManager);
        var definition = CreateDefinition();
        definition = new TemplateDefinitionDto
        {
            Name = definition.Name,
            Content = "Different content",
            Description = definition.Description,
        };

        var result = await TemplateManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            definition);

        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        documentManager.Verify(
            manager => manager.UpdateAsync(It.IsAny<TemplatesDocument>(), It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewDefinition_PersistsCompleteTemplate()
    {
        var document = new TemplatesDocument();
        var templatesManager = CreateManager(document, out var documentManager);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.PathBase = "/tenant-a";

        var result = await TemplateManagementEndpoints.CreateAsync(
            httpContext,
            CreateAuthorizationService(),
            templatesManager,
            CreateDefinition());

        var created = Assert.IsType<Created<TemplateDefinitionDto>>(result);
        Assert.Equal("/tenant-a/api/templates/Content__Article", created.Location);
        var template = Assert.Contains("Content__Article", document.Templates);
        Assert.Equal("<article>{{ Model.Content | shape_render }}</article>", template.Content);
        Assert.Equal("Article detail template.", template.Description);
        documentManager.Verify(
            manager => manager.UpdateAsync(document, It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RouteBodyNameMismatch_ReturnsBadRequestWithoutMutation()
    {
        var document = CreateDocument();
        var templatesManager = CreateManager(document, out var documentManager);

        var result = await TemplateManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            "Content__Article",
            CreateDefinition("Content__RenamedArticle"));

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        documentManager.Verify(
            manager => manager.UpdateAsync(It.IsAny<TemplatesDocument>(), It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExactRetry_ReturnsCurrentRepresentation()
    {
        var document = CreateDocument();
        var templatesManager = CreateManager(document, out var documentManager);
        var definition = new TemplateDefinitionDto
        {
            Name = "Content__Article",
            Content = "<article>Updated</article>",
            Description = "Updated description.",
        };

        var first = await TemplateManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            "Content__Article",
            definition);
        var retry = await TemplateManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            "Content__Article",
            definition);

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(first).StatusCode);
        var response = Assert.IsType<Ok<TemplateDefinitionDto>>(retry).Value;
        Assert.Equal(definition.Content, response.Content);
        Assert.Equal(definition.Description, response.Description);
        documentManager.Verify(
            manager => manager.UpdateAsync(document, It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteAsync_RetryAfterDelete_ReturnsNoContentWithoutSecondMutation()
    {
        var document = CreateDocument();
        var templatesManager = CreateManager(document, out var documentManager);

        var first = await TemplateManagementEndpoints.DeleteAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            "content__article");
        var retry = await TemplateManagementEndpoints.DeleteAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            "Content__Article");

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(first).StatusCode);
        Assert.IsType<NoContent>(retry);
        documentManager.Verify(
            manager => manager.UpdateAsync(document, It.IsAny<Func<TemplatesDocument, Task>>()),
            Times.Once);
    }

    [Fact]
    public async Task ListAsync_SearchAndPaging_ReturnsLowerCamelPagedEnvelope()
    {
        var document = CreateDocument();
        document.Templates["Widget__CallToAction"] = new Template
        {
            Content = "<a>{{ Model.Text }}</a>",
            Description = "Marketing call to action.",
        };
        var templatesManager = CreateManager(document, out _);

        var result = await TemplateManagementEndpoints.ListAsync(
            new DefaultHttpContext(),
            CreateAuthorizationService(),
            templatesManager,
            new TemplateListRequest { Search = "article", Skip = 0, Take = 1 });

        var response = Assert.IsType<Ok<TemplateListResponse>>(result).Value;
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("Content__Article", Assert.Single(response.Items).Name);

        var json = JsonSerializer.Serialize(response, JsonSerializerOptions.Web);
        using var parsed = JsonDocument.Parse(json);
        Assert.True(parsed.RootElement.TryGetProperty("skip", out _));
        Assert.True(parsed.RootElement.TryGetProperty("take", out _));
        Assert.True(parsed.RootElement.TryGetProperty("totalCount", out _));
        Assert.True(parsed.RootElement.TryGetProperty("items", out _));
    }

    [Fact]
    public async Task GetAsync_MissingPermission_ReturnsForbidden()
    {
        var result = await TemplateManagementEndpoints.GetAsync(
            CreateHttpContext(),
            CreateAuthorizationService(succeeds: false),
            CreateManager(CreateDocument(), out _),
            "Content__Article");

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    private static TemplatesDocument CreateDocument() =>
        new()
        {
            Templates =
            {
                ["Content__Article"] = new Template
                {
                    Content = "<article>{{ Model.Content | shape_render }}</article>",
                    Description = "Article detail template.",
                },
            },
        };

    private static TemplateDefinitionDto CreateDefinition(string name = "Content__Article") =>
        new()
        {
            Name = name,
            Content = "<article>{{ Model.Content | shape_render }}</article>",
            Description = "Article detail template.",
        };

    private static TemplatesManager CreateManager(
        TemplatesDocument document,
        out Mock<IDocumentManager<TemplatesDocument>> documentManager)
    {
        documentManager = new Mock<IDocumentManager<TemplatesDocument>>();
        documentManager
            .Setup(manager => manager.GetOrCreateImmutableAsync(It.IsAny<Func<Task<TemplatesDocument>>>()))
            .ReturnsAsync(document);
        documentManager
            .Setup(manager => manager.GetOrCreateMutableAsync(It.IsAny<Func<Task<TemplatesDocument>>>()))
            .ReturnsAsync(document);
        documentManager
            .Setup(manager => manager.UpdateAsync(document, It.IsAny<Func<TemplatesDocument, Task>>()))
            .Returns(Task.CompletedTask);

        return new TemplatesManager(documentManager.Object);
    }

    private static IAuthorizationService CreateAuthorizationService(bool succeeds = true)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        return authorizationService.Object;
    }

    private static DefaultHttpContext CreateHttpContext() =>
        new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .AddLocalization()
                .AddProblemDetails()
                .BuildServiceProvider(),
        };
}
