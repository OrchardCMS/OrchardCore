using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Environment.Shell;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;
using Microsoft.AspNetCore.Mvc;
using RewriteAdminController = OrchardCore.UrlRewriting.Controllers.AdminController;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Endpoints.Management;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class UrlRewriteEndpointTests : IDisposable
{
    private readonly ServiceProvider _services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task RuntimeRewrite_KeepsTheTenantPrefixAndTargetAuthorization()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await FeatureAsync(site, true);
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(), manager, new RewriteDefinition
            {
                Id = "tenant-route", Name = "Tenant route", Source = "Rewrite", Pattern = "^/prefix-probe$",
                SubstitutionPattern = "/api/url-rewriting/rules", SkipFurtherRules = true,
            });
        });
        using var direct = await site.Client.GetAsync("api/url-rewriting/rules", TestContext.Current.CancellationToken);
        using var rewritten = await site.Client.GetAsync("prefix-probe", TestContext.Current.CancellationToken);
        Assert.NotEqual(System.Net.HttpStatusCode.NotFound, direct.StatusCode);
        Assert.Equal(direct.StatusCode, rewritten.StatusCode);
        if (direct.IsSuccessStatusCode)
        {
            Assert.Equal(await direct.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                await rewritten.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData("^changed$", true)]
    [InlineData("(", false)]
    public async Task ExistingAdmin_UsesManagerValidationBeforePersisting(string pattern, bool valid)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await FeatureAsync(site, true);
        await site.UsingTenantScopeAsync(async scope =>
        {
            await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(),
                scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>(), Definition("admin-rule", "/target"));
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var display = new Mock<IDisplayManager<RewriteRule>>();
            display.Setup(service => service.UpdateEditorAsync(It.IsAny<RewriteRule>(), It.IsAny<IUpdateModel>(), false, "", ""))
                .Callback((RewriteRule rule, IUpdateModel _, bool _, string _, string _) =>
                    rule.Put(new UrlRewriteSourceMetadata { Pattern = pattern, SubstitutionPattern = "/target" }))
                .ReturnsAsync(Mock.Of<IShape>());
            var accessor = new Mock<IUpdateModelAccessor>();
            accessor.SetupGet(value => value.ModelUpdater).Returns(Mock.Of<IUpdateModel>());
            var controller = ActivatorUtilities.CreateInstance<RewriteAdminController>(scope.ServiceProvider,
                display.Object, accessor.Object, Authorized(), Mock.Of<INotifier>());
            controller.ControllerContext = new ControllerContext { HttpContext = Http() };
            controller.Url = Mock.Of<IUrlHelper>();
            controller.TempData = Mock.Of<global::Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
            var result = await controller.EditPOST("admin-rule");
            if (valid)
            {
                Assert.IsType<RedirectToActionResult>(result);
            }
            else
            {
                Assert.IsType<ViewResult>(result);
                Assert.False(controller.ModelState.IsValid);
            }
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var rule = await scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>().FindByIdAsync("admin-rule");
            Assert.Equal(valid ? pattern : "^original$", rule.GetOrCreate<UrlRewriteSourceMetadata>().Pattern);
        });
    }

    [Fact]
    public async Task Definitions_RetryReplaceRejectInvalidAndPersistOrder()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await FeatureAsync(site, true);
        var definition = Definition("first", "/target");
        string createdJson = null;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var result = Assert.IsType<Created<RewriteResponse>>(await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(),
                scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>(), definition));
            createdJson = System.Text.Json.JsonSerializer.Serialize(result.Value);
            Assert.Equal("first", result.Value.Id);
            Assert.Equal("/target", result.Value.Definition.SubstitutionPattern);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            var retry = Assert.IsType<Ok<RewriteResponse>>(await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(), manager, definition));
            Assert.Equal(createdJson, System.Text.Json.JsonSerializer.Serialize(retry.Value));
            Assert.Equal(409, Status(await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(), manager, Definition("first", "/different"))));
            Assert.Equal(400, Status(await RewriteManagementEndpoints.UpdateAsync(Http(), Authorized(), manager, "first", Definition("first", "/bad\npath"))));
            Assert.Equal("/target", (await manager.FindByIdAsync("first")).Properties["UrlRewriteSourceMetadata"]["SubstitutionPattern"].GetValue<string>());
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            Assert.IsType<Created<RewriteResponse>>(await RewriteManagementEndpoints.CreateAsync(Http(), Authorized(),
                scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>(), Definition("second", "/second")));
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            var moved = Assert.IsType<Ok<RewriteResponse>>(await RewriteManagementEndpoints.MoveAsync(Http(), Authorized(), manager,
                "first", new() { Position = 1 }));
            Assert.Equal(1, moved.Value.Order);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            Assert.Equal(new[] { "second", "first" }, (await manager.GetAllAsync()).Select(rule => rule.Id));
            var updated = Assert.IsType<Ok<RewriteResponse>>(await RewriteManagementEndpoints.UpdateAsync(Http(), Authorized(), manager,
                "first", Definition("first", "/updated")));
            Assert.Equal(1, updated.Value.Order);
            Assert.Equal("/updated", updated.Value.Definition.SubstitutionPattern);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            var shown = Assert.IsType<Ok<RewriteResponse>>(await RewriteManagementEndpoints.GetAsync(Http(), Authorized(), manager, "first"));
            Assert.Equal("/updated", shown.Value.Definition.SubstitutionPattern);
            Assert.IsType<NoContent>(await RewriteManagementEndpoints.DeleteAsync(Http(), Authorized(), manager, "first"));
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();
            Assert.IsType<NoContent>(await RewriteManagementEndpoints.DeleteAsync(Http(), Authorized(), manager, "first"));
            Assert.Equal(404, Status(await RewriteManagementEndpoints.GetAsync(Http(), Authorized(), manager, "first")));
        });
    }

    [Fact]
    public async Task EveryOperation_RequiresResourcePermissionBeforeAccessingStore()
    {
        var manager = new Mock<IRewriteRulesManager>(MockBehavior.Strict).Object;
        var auth = Authorize(RemoteManagementPermissions.AccessRemoteManagement);
        foreach (var result in new[]
        {
            await RewriteManagementEndpoints.ListAsync(Http(), auth, manager),
            await RewriteManagementEndpoints.GetAsync(Http(), auth, manager, "id"),
            await RewriteManagementEndpoints.SourcesAsync(Http(), auth, []),
            await RewriteManagementEndpoints.ValidateAsync(Http(), auth, manager, Definition("id", "/target")),
            await RewriteManagementEndpoints.CreateAsync(Http(), auth, manager, Definition("id", "/target")),
            await RewriteManagementEndpoints.UpdateAsync(Http(), auth, manager, "id", Definition("id", "/target")),
            await RewriteManagementEndpoints.DeleteAsync(Http(), auth, manager, "id"),
            await RewriteManagementEndpoints.MoveAsync(Http(), auth, manager, "id", new() { Position = 0 }),
        })
        {
            Assert.Equal(403, Status(result));
        }
    }

    private static RewriteDefinition Definition(string id, string substitution) => new()
    {
        Id = id, Name = id, Source = "Rewrite", Pattern = "^original$", SubstitutionPattern = substitution,
    };

    private DefaultHttpContext Http() => new() { RequestServices = _services };
    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;
    private static IAuthorizationService Authorized() => Authorize(RemoteManagementPermissions.AccessRemoteManagement, UrlRewritingPermissions.ManageUrlRewritingRules);
    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var auth = new Mock<IAuthorizationService>();
        auth.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return auth.Object;
    }

    private static Task FeatureAsync(SiteContext site, bool enabled) => site.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.UrlRewriting");
        if (enabled)
        {
            await manager.EnableFeaturesAsync([feature], force: true);
        }
        else
        {
            await manager.DisableFeaturesAsync([feature], force: true);
        }
    });
}
