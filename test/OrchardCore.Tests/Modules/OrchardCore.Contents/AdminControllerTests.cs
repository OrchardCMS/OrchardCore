using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using YesSql.Filters.Query;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class AdminControllerTests
{
    [Fact]
    public async Task ListShouldDeduplicateContentTypesAndIgnoreStereotypes()
    {
        var (controller, queryService) = CreateController();
        var filterResult = CreateFilterResult();
        var options = new ContentOptionsViewModel();
        queryService
            .Setup(service => service.QueryAsync(options, controller))
            .ThrowsAsync(new QueryReachedException());

        await Assert.ThrowsAsync<QueryReachedException>(() => controller.List(
            Options.Create(new PagerOptions()),
            Mock.Of<IShapeFactory>(),
            queryService.Object,
            filterResult,
            options,
            new PagerParameters(),
            ["Article", "Article", "BlogPost"],
            ["Page"]));

        var contentTypeNode = Assert.Single(filterResult.OfType<ContentTypeFilterNode>());
        Assert.Equal("Article,BlogPost", contentTypeNode.Operation.ToString());
        Assert.Empty(filterResult.OfType<StereotypeFilterNode>());
    }

    [Fact]
    public async Task ListShouldPreserveSingleContentTypeBehavior()
    {
        var (controller, queryService) = CreateController();
        var filterResult = CreateFilterResult();
        var options = new ContentOptionsViewModel();
        queryService
            .Setup(service => service.QueryAsync(options, controller))
            .ThrowsAsync(new QueryReachedException());

        await Assert.ThrowsAsync<QueryReachedException>(() => controller.List(
            Options.Create(new PagerOptions()),
            Mock.Of<IShapeFactory>(),
            queryService.Object,
            filterResult,
            options,
            new PagerParameters(),
            ["Article"],
            ["Page"]));

        Assert.Equal("Article", options.SelectedContentType);
        Assert.Equal("Article", Assert.Single(filterResult.OfType<ContentTypeFilterNode>()).Operation.ToString());
        Assert.Empty(filterResult.OfType<StereotypeFilterNode>());
    }

    [Fact]
    public async Task ListShouldDeduplicateStereotypesIgnoringCase()
    {
        var (controller, queryService) = CreateController();
        var filterResult = CreateFilterResult();
        var options = new ContentOptionsViewModel();
        queryService
            .Setup(service => service.QueryAsync(options, controller))
            .ThrowsAsync(new QueryReachedException());

        await Assert.ThrowsAsync<QueryReachedException>(() => controller.List(
            Options.Create(new PagerOptions()),
            Mock.Of<IShapeFactory>(),
            queryService.Object,
            filterResult,
            options,
            new PagerParameters(),
            null,
            ["Page", "page", "Widget"]));

        Assert.Empty(filterResult.OfType<ContentTypeFilterNode>());
        Assert.Equal("Page,Widget", Assert.Single(filterResult.OfType<StereotypeFilterNode>()).Operation.ToString());
    }

    private static (AdminController Controller, Mock<IContentsAdminListQueryService> QueryService) CreateController()
    {
        var definitions = new[]
        {
            new ContentTypeDefinitionBuilder()
                .WithName("Article")
                .WithDisplayName("Article")
                .Listable()
                .Creatable()
                .Build(),
            new ContentTypeDefinitionBuilder()
                .WithName("BlogPost")
                .WithDisplayName("Blog Post")
                .Listable()
                .Creatable()
                .Build(),
        };

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager
            .Setup(manager => manager.ListTypeDefinitionsAsync())
            .ReturnsAsync(definitions);
        definitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => definitions.FirstOrDefault(definition => definition.Name == name));

        var stringLocalizer = new Mock<IStringLocalizer<AdminController>>();
        stringLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var controller = new AdminController(
            authorizationService.Object,
            Mock.Of<IContentManager>(),
            Mock.Of<IContentItemDisplayManager>(),
            definitionManager.Object,
            Mock.Of<IDisplayManager<ContentOptionsViewModel>>(),
            Mock.Of<YesSql.ISession>(),
            Mock.Of<INotifier>(),
            Mock.Of<IHtmlLocalizer<AdminController>>(),
            stringLocalizer.Object)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "user-id")],
                        "Test")),
                },
            },
        };

        return (controller, new Mock<IContentsAdminListQueryService>());
    }

    private static QueryFilterResult<ContentItem> CreateFilterResult()
    {
        var builder = new QueryEngineBuilder<ContentItem>();
        new DefaultContentsAdminListFilterProvider().Build(builder);

        return builder.Build().Parse(string.Empty);
    }

    private sealed class QueryReachedException : Exception;
}
