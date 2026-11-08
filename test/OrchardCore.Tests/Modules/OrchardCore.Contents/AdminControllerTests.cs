using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using YesSql;
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

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(1, 0, 0)]
    [InlineData(3, 5, 25)]
    [InlineData(50, 0, 490)]
    [InlineData(1, 10, 1000)]
    [InlineData(3, 10, 1000)]
    [InlineData(101, 5, 1000)]
    [InlineData(200, 0, 1000)]
    public async Task List_MaxPagedCountSet_ReportsTotalWithoutCountingItems(int page, int contentItemsCount, int expectedTotalItemCount)
    {
        var query = CreateQuery(contentItemsCount);

        var (options, pagerShape) = await ListAsync(query, new PagerOptions { PageSize = 10, MaxPagedCount = 1000 }, page);

        Assert.Equal(contentItemsCount, options.ContentItemsCount);
        Assert.Equal(expectedTotalItemCount, options.TotalItemCount);
        Assert.Equal(expectedTotalItemCount, pagerShape.Properties["TotalItemCount"]);
        query.Verify(value => value.CountAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task List_MaxPagedCountNotSet_CountsItems()
    {
        var query = CreateQuery(5);
        query
            .Setup(value => value.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        var (options, pagerShape) = await ListAsync(query, new PagerOptions { PageSize = 10 }, 1);

        Assert.Equal(25, options.TotalItemCount);
        Assert.Equal(25, pagerShape.Properties["TotalItemCount"]);
    }

    private static async Task<(ContentOptionsViewModel Options, IShape PagerShape)> ListAsync(Mock<IQuery<ContentItem>> query, PagerOptions pagerOptions, int page)
    {
        var (controller, queryService) = CreateController();
        var options = new ContentOptionsViewModel();
        var shapeFactory = new TestShapeFactory();
        queryService
            .Setup(service => service.QueryAsync(options, controller))
            .ReturnsAsync(query.Object);

        await Assert.ThrowsAsync<HeaderReachedException>(() => controller.List(
            Options.Create(pagerOptions),
            shapeFactory,
            queryService.Object,
            CreateFilterResult(),
            options,
            new PagerParameters { Page = page }));

        return (options, shapeFactory.Shape);
    }

    private static Mock<IQuery<ContentItem>> CreateQuery(int contentItemsCount)
    {
        var query = new Mock<IQuery<ContentItem>>();
        query
            .Setup(value => value.Skip(It.IsAny<int>()))
            .Returns(query.Object);
        query
            .Setup(value => value.Take(It.IsAny<int>()))
            .Returns(query.Object);
        query
            .Setup(value => value.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(0, contentItemsCount).Select(_ => new ContentItem()).ToList());

        return query;
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

        var contentManager = new Mock<IContentManager> { DefaultValue = DefaultValue.Mock };
        contentManager
            .Setup(manager => manager.LoadAsync(It.IsAny<ContentItem>()))
            .ReturnsAsync((ContentItem contentItem) => contentItem);

        var contentOptionsDisplayManager = new Mock<IDisplayManager<ContentOptionsViewModel>>();
        contentOptionsDisplayManager
            .Setup(manager => manager.BuildEditorAsync(
                It.IsAny<ContentOptionsViewModel>(),
                It.IsAny<IUpdateModel>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new HeaderReachedException());

        var controller = new AdminController(
            authorizationService.Object,
            contentManager.Object,
            Mock.Of<IContentItemDisplayManager>(),
            definitionManager.Object,
            contentOptionsDisplayManager.Object,
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

    private sealed class HeaderReachedException : Exception;

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => throw new NotSupportedException();

        public IShape Shape { get; private set; }

        public async ValueTask<IShape> CreateAsync(
            string shapeType, Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created)
        {
            Shape = await shapeFactory();
            created?.Invoke(new ShapeCreatedContext { Shape = Shape });

            return Shape;
        }
    }
}
