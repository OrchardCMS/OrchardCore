using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Placements.Endpoints.Management;
using OrchardCore.Placements.Services;
using OrchardCore.Placements.ViewModels;
using OrchardCore.Tests.Apis.Context;
using PlacementAdminController = OrchardCore.Placements.Controllers.AdminController;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class PlacementManagementTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rules_PersistThroughSelectedStoreAndRespectMatchingOrder(bool fileStorage)
    {
        using var context = await CreateContextAsync(fileStorage);
        var definition = new PlacementDefinition
        {
            ShapeType = "RemoteProbe", Nodes = Parse("""
            [
              {"place":"Content:1","displayType":"Detail","contentType":["Probe"],"alternates":["First"]},
              {"place":"Header:3","displayType":"Detail","path":"~/news*","alternates":["Second"],"wrappers":["Wrapper"]}
            ]
            """),
        };
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                Assert.Equal(fileStorage, scope.ServiceProvider.GetRequiredService<IPlacementStore>() is FilePlacementsStore);
                var result = await PlacementManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true),
                    scope.ServiceProvider.GetRequiredService<PlacementsManager>(), definition);
                Assert.Equal("RemoteProbe", Assert.IsType<Created<PlacementDefinition>>(result).Value.ShapeType);
            });
        }
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<PlacementsManager>();
            var stored = Assert.IsType<Ok<PlacementDefinition>>(await PlacementManagementEndpoints.GetAsync(
                new DefaultHttpContext(), Authorize(true), manager, "remoteprobe")).Value;
            Assert.Equal(2, stored.Nodes.Length);
            Assert.Contains("contentType", stored.Nodes[0].Filters.Keys);
            scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Path = "/news/story";
            var zone = new Shape();
            zone.Properties["ContentItem"] = new ContentItem { ContentType = "Probe" };
            var provider = scope.ServiceProvider.GetServices<IShapePlacementProvider>().OfType<PlacementProvider>().Single();
            var resolver = await provider.BuildPlacementInfoResolverAsync(null);
            var detail = resolver.ResolvePlacement(new ShapePlacementContext("RemoteProbe", "Detail", null, zone));
            Assert.Equal("Header:3", detail.Location);
            Assert.Equal(["First", "Second"], detail.Alternates);
            Assert.Equal(["Wrapper"], detail.Wrappers);
            Assert.Null(resolver.ResolvePlacement(new ShapePlacementContext("RemoteProbe", "Summary", null, zone)));
        });
        // Updating to an empty array removes the override, including on a retry after it is absent.
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope => Assert.IsType<NoContent>(await PlacementManagementEndpoints.UpdateAsync(
                new DefaultHttpContext(), Authorize(true), scope.ServiceProvider.GetRequiredService<PlacementsManager>(), "RemoteProbe",
                new() { ShapeType = "RemoteProbe", Nodes = [] })));
        }
        await context.UsingTenantScopeAsync(async scope => Assert.Null(await scope.ServiceProvider.GetRequiredService<PlacementsManager>().GetShapePlacementsAsync("RemoteProbe")));
    }

    [Fact]
    public async Task InvalidOrConflictingRules_DoNotOverwriteExistingPlacement()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<PlacementsManager>();
            Assert.IsType<Created<PlacementDefinition>>(await PlacementManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true), manager,
                new() { ShapeType = "Guarded", Nodes = Parse("""[{"place":"Content:1"}]""") }));
            Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await PlacementManagementEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(true), manager,
                new() { ShapeType = "guarded", Nodes = Parse("""[{"place":"Header:1"}]""") })).StatusCode);
            foreach (var json in new[] { "[{}]", "[null]", "[{\"place\":\"-\",\"contenttype\":\"Probe\"}]", "[{\"place\":\"-\",\"path\":null}]", "[{\"place\":\"-\",\"contentType\":42}]", "[{\"wrappers\":[null]}]" })
            {
                var invalid = new PlacementDefinition { ShapeType = "Guarded", Nodes = Parse(json) };
                Assert.False(Assert.IsType<Ok<PlacementValidationResponse>>(await PlacementManagementEndpoints.ValidateAsync(new DefaultHttpContext(), Authorize(true), manager, invalid)).Value.IsValid);
                Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await PlacementManagementEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(true), manager, "Guarded", invalid)).StatusCode);
            }
        });
        await context.UsingTenantScopeAsync(async scope => Assert.Equal("Content:1", Assert.Single(await scope.ServiceProvider.GetRequiredService<PlacementsManager>().GetShapePlacementsAsync("Guarded")).Location));
    }

    [Fact]
    public async Task AdminEdit_UsesSharedValidationAndPreservesEmptyArrayDeletion()
    {
        using var context = await CreateContextAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var controller = CreateController(scope.ServiceProvider);
            var model = new EditShapePlacementViewModel { Creating = true, ShapeType = " AdminProbe ", Nodes = "[{\"place\":\"Content:1\"}]" };
            Assert.IsType<RedirectToActionResult>(await controller.Edit(model, "Save"));
            Assert.False(model.Creating);
            Assert.Equal("AdminProbe", model.ShapeType);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var controller = CreateController(scope.ServiceProvider);
            Assert.IsType<ViewResult>(await controller.Edit(new EditShapePlacementViewModel { ShapeType = "AdminProbe", Nodes = "[{\"displayType\":\"Detail\"}]" }, "Save"));
            Assert.False(controller.ModelState.IsValid);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<PlacementsManager>();
            Assert.Equal("Content:1", Assert.Single(await manager.GetShapePlacementsAsync("AdminProbe")).Location);
            Assert.IsType<RedirectToActionResult>(await CreateController(scope.ServiceProvider).Edit(new EditShapePlacementViewModel { ShapeType = "AdminProbe", Nodes = "[]" }, "Save"));
        });
        await context.UsingTenantScopeAsync(async scope => Assert.Null(await scope.ServiceProvider.GetRequiredService<PlacementsManager>().GetShapePlacementsAsync("AdminProbe")));
    }

    [Fact]
    public async Task DeniedOperations_DoNotReadOrWriteTheStore()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var auth = Authorize(false);
        IResult[] results =
        [
            await PlacementManagementEndpoints.ListAsync(http, auth, null, new()),
            await PlacementManagementEndpoints.GetAsync(http, auth, null, "name"),
            await PlacementManagementEndpoints.FiltersAsync(http, auth, null),
            await PlacementManagementEndpoints.ValidateAsync(http, auth, null, new()),
            await PlacementManagementEndpoints.CreateAsync(http, auth, null, new()),
            await PlacementManagementEndpoints.UpdateAsync(http, auth, null, "name", new()),
            await PlacementManagementEndpoints.DeleteAsync(http, auth, null, "name"),
        ];
        Assert.All(results, result => Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode));
    }

    private static PlacementNode[] Parse(string value) => JConvert.DeserializeObject<PlacementNode[]>(value);

    private static PlacementAdminController CreateController(IServiceProvider services)
    {
        var controller = ActivatorUtilities.CreateInstance<PlacementAdminController>(services, Authorize(true), Mock.Of<INotifier>());
        controller.ControllerContext = new() { HttpContext = new DefaultHttpContext { RequestServices = services } };
        controller.Url = Mock.Of<IUrlHelper>();
        return controller;
    }

    private static async Task<SiteContext> CreateContextAsync(bool fileStorage = false)
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = await manager.GetAvailableFeaturesAsync();
            await manager.EnableFeaturesAsync(features.Where(feature => feature.Id == (fileStorage ? "OrchardCore.Placements.FileStorage" : "OrchardCore.Placements")), force: true);
        });
        return context;
    }

    private static IAuthorizationService Authorize(bool success)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(success ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
