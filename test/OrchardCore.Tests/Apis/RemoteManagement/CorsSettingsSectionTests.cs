using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Tests.Apis.Context;
using CorsAdminController = OrchardCore.Cors.Controllers.AdminController;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Cors.Services;
using OrchardCore.Cors.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;
using TenantCorsService = OrchardCore.Cors.Services.CorsService;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class CorsSettingsSectionTests
{
    [Fact]
    public void Runtime_NoExplicitDefault_UsesFirstConfiguredPolicy()
    {
        var options = Configure(new CorsPolicySetting
        {
            Name = "First", AllowedOrigins = ["https://frontend.example.test"],
            AllowedMethods = ["GET"], AllowedHeaders = [],
        });

        Assert.Equal("First", options.DefaultPolicyName);
        Assert.NotNull(options.GetPolicy(options.DefaultPolicyName));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_UnsafeCredentialedWildcard_SkipsInvalidImportedPolicy(bool allowAnyOrigin)
    {
        var options = Configure(new CorsPolicySetting
        {
            Name = "Unsafe", AllowAnyOrigin = allowAnyOrigin, AllowCredentials = true,
            AllowedOrigins = ["*"], AllowedMethods = ["GET"], AllowedHeaders = [],
        }, new CorsPolicySetting
        {
            Name = "Safe", AllowedOrigins = ["https://frontend.example.test"],
            AllowedMethods = ["GET"], AllowedHeaders = [], IsDefaultPolicy = true,
        });

        Assert.Null(options.GetPolicy("Unsafe"));
        Assert.NotNull(options.GetPolicy("Safe"));
        Assert.Equal("Safe", options.DefaultPolicyName);
    }

    [Fact]
    public async Task Section_ReplacementOmissionAndEmptyArray_AreSafeAndIdempotent()
    {
        var site = new SiteSettings { SiteName = "Preserved", SiteSalt = "not-for-readback" };
        site.Properties["OtherSection"] = new JsonObject { ["value"] = 42 };
        var fixture = Create(site);
        var patch = JsonNode.Parse("""{"policies":[{"name":"Frontend","allowedOrigins":["https://frontend.example.test"],"allowedMethods":["GET"],"allowCredentials":true}]}""").AsObject();
        var first = await fixture.Provider.UpdateAsync(patch);
        Assert.True(first.Changed);
        Assert.True(first.ReloadRequested);
        Assert.Equal("tenant", first.Section.Source);
        Assert.DoesNotContain("not-for-readback", first.Section.Values.ToJsonString(), StringComparison.Ordinal);
        Assert.Equal("Preserved", site.SiteName);
        Assert.Equal(42, site.Properties["OtherSection"]["value"].GetValue<int>());
        Assert.Equal(first.Section.Values.ToJsonString(), (await fixture.Provider.GetAsync()).Values.ToJsonString());
        Assert.False((await fixture.Provider.UpdateAsync(patch)).Changed);
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Once);
        var cleared = await fixture.Provider.UpdateAsync(new JsonObject { ["policies"] = new JsonArray() });
        Assert.True(cleared.Changed);
        Assert.Empty(cleared.Section.Values["policies"].AsArray());
        Assert.False((await fixture.Provider.UpdateAsync(new JsonObject { ["policies"] = new JsonArray() })).Changed);
    }

    [Theory]
    [InlineData("{\"policies\":null}")]
    [InlineData("{\"policies\":[null]}")]
    [InlineData("{\"unknown\":true}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"unknown\":true}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":null}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowCredentials\":null}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedHeaders\":[\"X-Bad:Value\"]}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":[\"https://example.test/path\"]}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":[\"https://example.test/\"]}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":[\"https://example.test/a/..\"]}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":[\"https://*.example.test\"]}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"allowedOrigins\":[\"*\"],\"allowCredentials\":true}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\"},{\"name\":\"A\"}]}")]
    [InlineData("{\"policies\":[{\"name\":\"A\",\"isDefaultPolicy\":true},{\"name\":\"B\",\"isDefaultPolicy\":true}]}")]
    public async Task Section_InvalidPatch_DoesNotPersistOrReload(string json)
    {
        var fixture = Create(new SiteSettings());
        Assert.NotEmpty((await fixture.Provider.UpdateAsync(JsonNode.Parse(json).AsObject())).Errors);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Theory]
    [InlineData("null", false)]
    [InlineData("not-json", false)]
    [InlineData("[{\"name\":\"A\",\"allowAnyOrigin\":true,\"allowCredentials\":true}]", false)]
    [InlineData("[{\"name\":\"A\",\"allowedOrigins\":[\"https://example.test\"],\"allowedMethods\":[\"GET\"]}]", true)]
    public async Task Admin_UsesSharedValidationAndChangeDetection(string json, bool valid)
    {
        var fixture = Create(new SiteSettings());
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var html = new Mock<IHtmlLocalizer<CorsAdminController>>();
        html.Setup(localizer => localizer[It.IsAny<string>()]).Returns((string name) => new LocalizedHtmlString(name, name));
        var controller = new CorsAdminController(fixture.Release.Object, authorization.Object, fixture.Service, Mock.Of<INotifier>(), html.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext() },
        };
        controller.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["CorsSettings"] = json });
        Assert.IsType<ViewResult>(await controller.IndexPOST());
        Assert.Equal(valid, controller.ModelState.IsValid);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), valid ? Times.Once : Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), valid ? Times.Once : Times.Never);
        if (valid)
        {
            await controller.IndexPOST();
            fixture.Release.Verify(service => service.RequestRelease(), Times.Once);
        }
    }

    [Fact]
    public async Task Section_PersistsAcrossScopesAndFollowsFeatureLifecycle()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetFeatureAsync(context, true);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>().Single(provider => provider.Descriptor.Name == "cors");
            var result = await provider.UpdateAsync(JsonNode.Parse("""{"policies":[{"name":"Frontend","allowedOrigins":["https://frontend.example.test"],"allowedMethods":["GET"]}]}""").AsObject());
            Assert.True(result.Changed);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>().Single(provider => provider.Descriptor.Name == "cors");
            Assert.Equal("Frontend", (await provider.GetAsync()).Values["policies"][0]["name"].GetValue<string>());
        });
        await SetFeatureAsync(context, false);
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "cors");
            return Task.CompletedTask;
        });
    }

    private static Task SetFeatureAsync(SiteContext context, bool enabled) => context.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Cors");
        if (enabled)
        {
            await manager.EnableFeaturesAsync([feature], force: true);
        }
        else
        {
            await manager.DisableFeaturesAsync([feature], force: true);
        }
    });

    private static (TenantCorsService Service, CorsSettingsSectionProvider Provider, Mock<ISiteService> Site, Mock<IShellReleaseManager> Release) Create(SiteSettings site)
    {
        site.IsReadOnly = false; // LoadSiteSettingsAsync returns a mutable document in the real service.
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var service = new TenantCorsService(siteService.Object, new StringLocalizer<TenantCorsService>(new NullStringLocalizerFactory()));
        var release = new Mock<IShellReleaseManager>();
        return (service, new CorsSettingsSectionProvider(service, release.Object), siteService, release);
    }

    private static CorsOptions Configure(params CorsPolicySetting[] policies)
    {
        var site = new SiteSettings();
        site.Put(nameof(CorsSettings), new CorsSettings { Policies = policies });
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        var configuration = new CorsOptionsConfiguration(new TenantCorsService(siteService.Object, new StringLocalizer<TenantCorsService>(new NullStringLocalizerFactory())), NullLogger<CorsOptionsConfiguration>.Instance);
        var options = new CorsOptions();
        configuration.Configure(options);
        return options;
    }
}
