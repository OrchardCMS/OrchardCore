using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Https.Drivers;
using OrchardCore.Https.Services;
using OrchardCore.Https.Settings;
using OrchardCore.Https.ViewModels;
using OrchardCore.RemoteManagement;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class TypedSettingsSectionTests
{
    [Fact]
    public async Task Https_PartialUpdatesNullResetAndRetries_PreserveOtherSettings()
    {
        var site = new SiteSettings { SiteName = "Unchanged", SiteSalt = "private-site-salt" };
        site.Properties["Unrelated"] = new JsonObject { ["value"] = "preserved" };
        site.Put(nameof(HttpsSettings), new HttpsSettings { RequireHttpsPermanent = true });
        var fixture = CreateProvider(site);
        var first = await fixture.Provider.UpdateAsync(new JsonObject { ["requireHttps"] = true, ["sslPort"] = 8443 });
        Assert.True(first.Changed);
        Assert.True(first.ReloadRequested);
        Assert.True(first.Section.Values["requireHttpsPermanent"].GetValue<bool>());
        Assert.Equal(8443, first.Section.Values["sslPort"].GetValue<int>());
        Assert.DoesNotContain("private-site-salt", first.Section.Values.ToJsonString(), StringComparison.Ordinal);
        Assert.Equal("Unchanged", site.SiteName);
        Assert.Equal("preserved", site.Properties["Unrelated"]["value"].GetValue<string>());
        var again = await fixture.Provider.UpdateAsync(new JsonObject { ["requireHttps"] = true, ["sslPort"] = 8443 });
        Assert.False(again.Changed);
        Assert.False(again.ReloadRequested);
        Assert.True(JsonNode.DeepEquals(first.Section.Values, again.Section.Values));
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Once);
        var reset = await fixture.Provider.UpdateAsync(new JsonObject { ["sslPort"] = null });
        Assert.Null(reset.Section.Values["sslPort"]);
        Assert.True(reset.Section.Values["requireHttps"].GetValue<bool>());
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
    }

    [Theory]
    [InlineData("{\"sslPort\":0}")]
    [InlineData("{\"sslPort\":65536}")]
    [InlineData("{\"sslPort\":1.5}")]
    [InlineData("{\"requireHttps\":null}")]
    [InlineData("{\"requireHttpsPermanent\":\"true\"}")]
    [InlineData("{\"strictTransportSecurityMode\":1}")]
    [InlineData("{\"strictTransportSecurityMode\":\"unknown\"}")]
    [InlineData("{\"siteSalt\":\"not-a-setting\"}")]
    [InlineData("{\"requireHttps\":true,\"unknown\":false}")]
    public async Task Https_InvalidPatch_DoesNotSaveOrReload(string json)
    {
        var site = new SiteSettings();
        var fixture = CreateProvider(site);
        var result = await fixture.Provider.UpdateAsync(JsonNode.Parse(json).AsObject());
        Assert.NotEmpty(result.Errors);
        Assert.False(site.GetOrCreate<HttpsSettings>().RequireHttps);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task SectionDiscoveryAndWrites_EnforcePermissionsTransportAndOwnership()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var fixture = CreateProvider(new SiteSettings());
        ISiteSettingsSectionProvider[] providers = [fixture.Provider];
        var accessOnly = Authorize(RemoteManagementPermissions.AccessRemoteManagement);
        var list = Assert.IsType<Ok<SiteSettingsSectionDescriptor[]>>(await SiteSettingsSectionEndpoints.ListAsync(http, accessOnly, providers));
        Assert.Empty(list.Value);
        foreach (var result in new[]
        {
            await SiteSettingsSectionEndpoints.GetAsync(http, accessOnly, providers, "https"),
            await SiteSettingsSectionEndpoints.SchemaAsync(http, accessOnly, providers, "https"),
            await SiteSettingsSectionEndpoints.UpdateAsync(http, accessOnly, providers, "https", []),
        })
        {
            Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }
        var authorized = Authorize(RemoteManagementPermissions.AccessRemoteManagement, global::OrchardCore.Https.Permissions.ManageHttps);
        Assert.Equal("https", Assert.Single(Assert.IsType<Ok<SiteSettingsSectionDescriptor[]>>(
            await SiteSettingsSectionEndpoints.ListAsync(http, authorized, providers)).Value).Name);
        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(
            await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized, providers, "https", new JsonObject { ["requireHttps"] = true })).StatusCode);
        fixture.Site.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
        Assert.IsType<Ok<SiteSettingsSectionResponse>>(await SiteSettingsSectionEndpoints.GetAsync(http, authorized, providers, "HTTPS"));
        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await SiteSettingsSectionEndpoints.GetAsync(http, authorized, [], "https")).StatusCode);
        http.Request.Scheme = "https";
        Assert.IsType<Ok<SiteSettingsSectionUpdateResult>>(await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized, providers, "https", []));

        var readOnly = new Mock<ISiteSettingsSectionProvider>(MockBehavior.Strict);
        readOnly.SetupGet(provider => provider.Descriptor).Returns(new SiteSettingsSectionDescriptor { Name = "host" });
        readOnly.SetupGet(provider => provider.ReadPermission).Returns(RemoteManagementPermissions.AccessRemoteManagement);
        readOnly.SetupGet(provider => provider.UpdatePermission).Returns(RemoteManagementPermissions.AccessRemoteManagement);
        readOnly.Setup(provider => provider.GetAsync()).ReturnsAsync(new SiteSettingsSectionResponse { Name = "host", Source = "configuration", IsReadOnly = true });
        Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized,
            [readOnly.Object], "host", new JsonObject { ["value"] = true })).StatusCode);
        readOnly.Setup(provider => provider.GetAsync()).ReturnsAsync(new SiteSettingsSectionResponse
        {
            Name = "host", Source = "mixed", ReadOnlyProperties = ["hostValue"],
        });
        Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized,
            [readOnly.Object], "host", new JsonObject { ["hostValue"] = true })).StatusCode);
        readOnly.Verify(provider => provider.UpdateAsync(It.IsAny<JsonObject>()), Times.Never);
    }

    [Theory]
    [InlineData("http", 8443, false)]
    [InlineData("https", 0, false)]
    [InlineData("https", 8443, true)]
    public async Task ExistingEditor_UsesSharedValidationAndOnlyReloadsOnChange(string scheme, int port, bool validChange)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(model => model.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<HttpsSettingsViewModel>(), It.IsAny<string>()))
            .Callback((HttpsSettingsViewModel model, string _) => { model.RequireHttps = true; model.SslPort = port; })
            .ReturnsAsync(true);
        var release = new Mock<IShellReleaseManager>();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Request.Scheme = scheme;
        var driver = new HttpsSettingsDisplayDriver(release.Object, accessor,
            Authorize(global::OrchardCore.Https.Permissions.ManageHttps), Mock.Of<INotifier>(),
            services.GetRequiredService<IStringLocalizer<HttpsSettingsDisplayDriver>>(), Mock.Of<IHtmlLocalizer<HttpsSettingsDisplayDriver>>());
        var context = new UpdateEditorContext(new Shape(), "Https", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var settings = new HttpsSettings();
        await driver.UpdateAsync(new SiteSettings(), settings, context);
        Assert.Equal(validChange, settings.RequireHttps);
        release.Verify(manager => manager.RequestRelease(), validChange ? Times.Once : Times.Never);
        if (validChange)
        {
            await driver.UpdateAsync(new SiteSettings(), settings, context);
            release.Verify(manager => manager.RequestRelease(), Times.Once);
        }
        if (scheme == "https" && !validChange)
        {
            Assert.False(updater.Object.ModelState.IsValid);
        }
    }

    [Fact]
    public async Task HttpsSettings_PersistAndDisappearWithTheirFeature()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetHttpsFeatureAsync(context, true);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "https");
            var result = await provider.UpdateAsync(new JsonObject { ["sslPort"] = 8443, ["strictTransportSecurityMode"] = "FromConfiguration" });
            Assert.True(result.Changed);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "https");
            var result = await provider.GetAsync();
            Assert.Equal(8443, result.Values["sslPort"].GetValue<int>());
            Assert.Equal("FromConfiguration", result.Values["strictTransportSecurityMode"].GetValue<string>());
        });
        await SetHttpsFeatureAsync(context, false);
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "https");
            return Task.CompletedTask;
        });
    }

    private static Task SetHttpsFeatureAsync(SiteContext context, bool enabled) => context.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Https");
        if (enabled)
        {
            await manager.EnableFeaturesAsync([feature], force: true);
        }
        else
        {
            await manager.DisableFeaturesAsync([feature], force: true);
        }
    });

    private static (HttpsSettingsSectionProvider Provider, Mock<ISiteService> Site, Mock<IShellReleaseManager> Release) CreateProvider(SiteSettings site)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        var release = new Mock<IShellReleaseManager>();
        var provider = new HttpsSettingsSectionProvider(siteService.Object, new HttpsService(siteService.Object), release.Object,
            new StringLocalizer<HttpsSettingsSectionProvider>(new NullStringLocalizerFactory()));
        return (provider, siteService, release);
    }

    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
