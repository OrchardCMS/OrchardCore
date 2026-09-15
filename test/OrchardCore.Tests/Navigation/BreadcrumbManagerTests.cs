using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Navigation;

public class BreadcrumbManagerTests
{
    private static readonly Permission _listContent = new("ListContent", "List content items");

    [Fact]
    public async Task BuildBreadcrumbAsync_WithNodesOfSeveralProviders_OrdersThemByPosition()
    {
        var manager = CreateManager(
            new DelegateBreadcrumbProvider(builder => builder.Add("Last", "after")),
            new DelegateBreadcrumbProvider(builder => builder.Add("First")));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Equal(["First", "Last"], items.Select(item => item.Text));
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithNodes_MarksTheLastOneAsCurrentWithoutLink()
    {
        var manager = CreateManager(new DelegateBreadcrumbProvider(builder => builder
            .Add("Manage Content", item => item.Url("/Admin/Contents/ContentItems"))
            .Add("Edit Site Page", item => item.Url("/Admin/Contents/ContentItems/Edit"))));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Collection(items,
            item =>
            {
                Assert.False(item.IsCurrent);
                Assert.Equal("/Admin/Contents/ContentItems", item.Href);
            },
            item =>
            {
                Assert.True(item.IsCurrent);
                Assert.Null(item.Href);
            });
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithUnauthorizedNode_KeepsTheNodeWithoutLink()
    {
        var manager = CreateManager(
            isAuthorized: false,
            new DelegateBreadcrumbProvider(builder => builder
                .Add("Manage Content", item => item
                    .Url("/Admin/Contents/ContentItems")
                    .Permission(_listContent))
                .Add("Edit Site Page")));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Equal(2, items.Count);
        Assert.Equal("Manage Content", items[0].Text);
        Assert.Null(items[0].Href);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithRelativeUrl_PrependsThePathBase()
    {
        var manager = CreateManager(new DelegateBreadcrumbProvider(builder => builder
            .Add("Manage Content", item => item.Url("~/Admin/Contents/ContentItems"))
            .Add("Edit Site Page")));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext(pathBase: "/tenant"));

        Assert.Equal("/tenant/Admin/Contents/ContentItems", items[0].Href);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithRouteValues_GeneratesTheUrlOfTheRoute()
    {
        var manager = CreateManager(new DelegateBreadcrumbProvider(builder => builder
            .Add("Manage Content", item => item.Action("List", "Admin", new { area = "OrchardCore.Contents" }))
            .Add("Edit Site Page")));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Equal("/OrchardCore.Contents/Admin/List", items[0].Href);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithFailingProvider_KeepsTheNodesOfTheOtherProviders()
    {
        var manager = CreateManager(
            new DelegateBreadcrumbProvider(_ => throw new InvalidOperationException("Boom.")),
            new DelegateBreadcrumbProvider(builder => builder.Add("Manage Content")));

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Equal("Manage Content", Assert.Single(items).Text);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithAnotherName_SkipsTheNamedProvider()
    {
        var manager = CreateManager(new StubNamedBreadcrumbProvider("ContentsEdit"));

        var items = await manager.BuildBreadcrumbAsync("Contents", CreateActionContext());

        Assert.Empty(items);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithData_GivesItToTheProviders()
    {
        var manager = CreateManager(new DelegateBreadcrumbProvider(builder =>
        {
            Assert.True(builder.TryGetData<string>("ContentType", out var contentType));

            builder.Add(contentType);
        }));

        var data = new Dictionary<string, object> { { "ContentType", "SitePage" } };

        var items = await manager.BuildBreadcrumbAsync("Contents", CreateActionContext(), data);

        Assert.Equal("SitePage", Assert.Single(items).Text);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithoutProvider_ReturnsNoNode()
    {
        var manager = CreateManager();

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext());

        Assert.Empty(items);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithInlineItems_SeedsThemBeforeTheProvidersRun()
    {
        // A provider still runs over an inline trail, so a module can extend it. Here it prepends a node the way the
        // Admin Dashboard feature does.
        var manager = CreateManager(new DelegateBreadcrumbProvider(builder => builder.Add("Dashboard", "start")));

        var inlineItems = new List<BreadcrumbItem>
        {
            new() { Text = "Manage Content" },
            new() { Text = "Edit Article" },
        };

        var items = await manager.BuildBreadcrumbAsync("ContentsEdit", CreateActionContext(), data: null, items: inlineItems);

        Assert.Equal(["Dashboard", "Manage Content", "Edit Article"], items.Select(item => item.Text));
    }

    private static BreadcrumbManager CreateManager(params IBreadcrumbProvider[] providers)
        => CreateManager(isAuthorized: true, providers);

    private static BreadcrumbManager CreateManager(bool isAuthorized, params IBreadcrumbProvider[] providers)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(isAuthorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(helper => helper.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                var values = (RouteValueDictionary)context.Values;

                return $"/{values["area"]}/{values["controller"]}/{values["action"]}";
            });

        var urlHelperFactory = new Mock<IUrlHelperFactory>();
        urlHelperFactory
            .Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelper.Object);

        return new BreadcrumbManager(
            providers,
            urlHelperFactory.Object,
            authorizationService.Object,
            NullLogger<BreadcrumbManager>.Instance);
    }

    private static ActionContext CreateActionContext(string pathBase = "")
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
        };

        httpContext.Request.PathBase = pathBase;

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    private sealed class DelegateBreadcrumbProvider : IBreadcrumbProvider
    {
        private readonly Action<BreadcrumbBuilder> _build;

        public DelegateBreadcrumbProvider(Action<BreadcrumbBuilder> build)
        {
            _build = build;
        }

        public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
        {
            _build(builder);

            return ValueTask.CompletedTask;
        }
    }

    private sealed class StubNamedBreadcrumbProvider : NamedBreadcrumbProvider
    {
        public StubNamedBreadcrumbProvider(string name)
            : base(name)
        {
        }

        protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
        {
            builder.Add("Manage Content");

            return ValueTask.CompletedTask;
        }
    }
}
