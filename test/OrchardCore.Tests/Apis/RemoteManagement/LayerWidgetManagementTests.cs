using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.Documents;
using OrchardCore.Entities;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.Endpoints.Management;
using OrchardCore.Security;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Title.Models;
using LayerAdminController = OrchardCore.Layers.Controllers.AdminController;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class LayerWidgetManagementTests
{
    [Fact]
    public async Task Placement_AttachMoveAndRetry_PreservesDraftAndPublishedBodies()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await CreateWidgetAsync(context);
        string stateId = null;
        for (var retry = 0; retry < 2; retry++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var services = scope.ServiceProvider;
                var service = CreateService(services, Authorize());
                var response = await LayerWidgetEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(), service, id,
                    new() { Layer = "target", Zone = "Content", Position = 3, RenderTitle = true });
                Assert.Equal("Target", Assert.IsType<Ok<LayerWidgetPlacement>>(response).Value.Layer);
            });
            await context.UsingTenantScopeAsync(async scope =>
            {
                var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                var latest = await manager.GetAsync(id, VersionOptions.Latest);
                var published = await manager.GetAsync(id, VersionOptions.Published);
                Assert.NotEqual(latest.ContentItemVersionId, published.ContentItemVersionId);
                Assert.False(latest.Published);
                Assert.True(published.Published);
                Assert.Equal("draft-body", latest.Get<TitlePart>(nameof(TitlePart)).Title);
                Assert.Equal("published-body", published.Get<TitlePart>(nameof(TitlePart)).Title);
                foreach (var item in new[] { latest, published })
                {
                    var placement = item.Get<LayerMetadata>(nameof(LayerMetadata));
                    Assert.Equal("Target", placement.Layer);
                    Assert.Equal(3, placement.Position);
                    Assert.True(placement.RenderTitle);
                }
                var state = await scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<LayerState>>().GetOrCreateImmutableAsync();
                if (stateId is not null)
                {
                    Assert.Equal(stateId, state.Identifier);
                }
                stateId = state.Identifier;
            });
        }
        await context.UsingTenantScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var controller = ActivatorUtilities.CreateInstance<LayerAdminController>(services, Authorize(), CreateService(services, Authorize()));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } };
            controller.Request.Headers.XRequestedWith = "XMLHttpRequest";
            Assert.IsType<OkResult>(await controller.UpdatePosition(id, -2, "Footer"));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            foreach (var version in new[] { VersionOptions.Latest, VersionOptions.Published })
            {
                var placement = (await manager.GetAsync(id, version)).Get<LayerMetadata>(nameof(LayerMetadata));
                Assert.Equal(-2, placement.Position);
                Assert.Equal("Footer", placement.Zone);
                Assert.Equal("Target", placement.Layer);
                Assert.True(placement.RenderTitle);
            }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Placement_DeniedPublishedVersion_DoesNotMutateEitherVersion(bool denyPublish)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await CreateWidgetAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var auth = Authorize((item, requirement) => item?.Published != true
                || (denyPublish ? requirement.Permission.Name != CommonPermissions.PublishContent.Name
                    : requirement.Permission.Name != CommonPermissions.EditContent.Name));
            var result = await CreateService(scope.ServiceProvider, auth).UpdateAsync(new ClaimsPrincipal(), id,
                new LayerMetadata { Layer = "Target", Zone = "Content", Position = 1 });
            Assert.Equal(LayerWidgetMutationStatus.Forbidden, result.Status);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            Assert.False((await manager.GetAsync(id, VersionOptions.Latest)).Has<LayerMetadata>());
            Assert.False((await manager.GetAsync(id, VersionOptions.Published)).Has<LayerMetadata>());
        });
    }

    [Fact]
    public async Task Placement_InvalidLayerZonePositionAndType_LeavesContentUnchanged()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await CreateWidgetAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = CreateService(scope.ServiceProvider, Authorize());
            foreach (var placement in new[]
            {
                new LayerMetadata { Layer = "missing", Zone = "Content" },
                new LayerMetadata { Layer = "Target", Zone = "content" },
                new LayerMetadata { Layer = "Target", Zone = "Content", Position = double.NaN },
                new LayerMetadata { Layer = "Target", Zone = "Content", Position = double.PositiveInfinity },
            })
            {
                Assert.Equal(LayerWidgetMutationStatus.Invalid, (await service.UpdateAsync(new ClaimsPrincipal(), id, placement)).Status);
            }
            Assert.Equal(LayerWidgetMutationStatus.Invalid, (await service.UpdateAsync(new ClaimsPrincipal(), id,
                new LayerMetadata { Zone = "Content" }, positionOnly: true)).Status);
            Assert.Equal(LayerWidgetMutationStatus.NotFound, (await service.UpdateAsync(new ClaimsPrincipal(), "absent",
                new LayerMetadata { Layer = "Target", Zone = "Content" })).Status);
            await scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>().AlterTypeDefinitionAsync("PlacementProbe", type => type.Stereotype("Content"));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var result = await CreateService(scope.ServiceProvider, Authorize()).UpdateAsync(new ClaimsPrincipal(), id,
                new LayerMetadata { Layer = "Target", Zone = "Content" });
            Assert.Equal(LayerWidgetMutationStatus.Invalid, result.Status);
            Assert.False((await scope.ServiceProvider.GetRequiredService<IContentManager>().GetAsync(id, VersionOptions.Latest)).Has<LayerMetadata>());
        });
    }

    [Fact]
    public async Task Reads_ApplyVersionPermissionsBeforeTotalsAndPaging()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await CreateWidgetAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            await CreateService(scope.ServiceProvider, Authorize()).UpdateAsync(new ClaimsPrincipal(), id,
                new LayerMetadata { Layer = "Target", Zone = "Content", Position = 1 });
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var auth = Authorize((item, requirement) => requirement.Permission.Name != CommonPermissions.PreviewContent.Name);
            var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            var layers = scope.ServiceProvider.GetRequiredService<ILayerService>();
            var latest = Assert.IsType<Ok<LayerWidgetListResponse>>(await LayerWidgetEndpoints.ListAsync(http, auth, layers, new() { Layer = "Target" })).Value;
            Assert.Equal(0, latest.TotalCount);
            Assert.Empty(latest.Items);
            var published = Assert.IsType<Ok<LayerWidgetListResponse>>(await LayerWidgetEndpoints.ListAsync(http, auth, layers,
                new() { Layer = "Target", Version = "published" })).Value;
            Assert.Equal(id, Assert.Single(published.Items).ContentItemId);
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await LayerWidgetEndpoints.GetAsync(http, auth, manager, id)).StatusCode);
            Assert.IsType<Ok<LayerWidgetResponse>>(await LayerWidgetEndpoints.GetAsync(http, auth, manager, id, "published"));
        });
    }

    [Fact]
    public async Task LayerDeletion_DraftMovedElsewhere_StillProtectsThePublishedReference()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var id = await CreateWidgetAsync(context);
        await context.UsingTenantScopeAsync(async scope =>
        {
            await CreateService(scope.ServiceProvider, Authorize()).UpdateAsync(new ClaimsPrincipal(), id,
                new LayerMetadata { Layer = "Target", Zone = "Content" });
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var draft = await manager.GetAsync(id, VersionOptions.Latest);
            draft.Alter<LayerMetadata>(placement => placement.Layer = "Other");
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(draft);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var layers = scope.ServiceProvider.GetRequiredService<ILayerService>();
            Assert.Equal(LayerMutationStatus.Referenced, (await layers.DeleteAsync("Target")).Status);
            Assert.NotNull(await layers.GetLayerAsync("Target"));
        });
    }

    [Fact]
    public async Task DeniedEndpoints_DoNotResolveResources()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var auth = Authorize((_, _) => false);
        IResult[] results =
        [
            await LayerWidgetEndpoints.ListAsync(http, auth, null, new()),
            await LayerWidgetEndpoints.GetAsync(http, auth, null, "id"),
            await LayerWidgetEndpoints.ZonesAsync(http, auth, null),
            await LayerWidgetEndpoints.UpdateAsync(http, auth, null, "id", new()),
        ];
        Assert.All(results, result => Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode));
    }

    private static async Task<string> CreateWidgetAsync(SiteContext context)
    {
        string id = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var definitions = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await definitions.AlterTypeDefinitionAsync("PlacementProbe", type => type.Stereotype("Widget").Versionable().Draftable().WithPart("TitlePart"));
            await scope.ServiceProvider.GetRequiredService<ILayerService>().CreateAsync("Target", "Test layer");
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Alter<LayerSettings>(settings => settings.Zones = ["Content", "Footer"]);
            await siteService.UpdateSiteSettingsAsync(site);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await manager.NewAsync("PlacementProbe");
            item.Alter<TitlePart>(part => part.Title = "published-body");
            await manager.CreateAsync(item, VersionOptions.Published);
            id = item.ContentItemId;
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await manager.GetAsync(id, VersionOptions.DraftRequired);
            item.Alter<TitlePart>(part => part.Title = "draft-body");
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(item);
        });
        return id;
    }

    private static LayerWidgetService CreateService(IServiceProvider services, IAuthorizationService authorization) =>
        ActivatorUtilities.CreateInstance<LayerWidgetService>(services, authorization);

    private static IAuthorizationService Authorize(Func<ContentItem, PermissionRequirement, bool> predicate = null)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => predicate?.Invoke(resource as ContentItem, requirement) != false)
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
