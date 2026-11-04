using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Layers.Drivers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.RemoteManagement;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class LayerZoneSettingsSectionTests
{
    [Theory]
    [InlineData("Header, Footer  Header,header")]
    [InlineData("")]
    [InlineData(null)]
    public async Task ExistingEditor_AndRemoteUpdates_UseTheSameZoneSemantics(string input)
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(model => model.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<LayerSettingsViewModel>(), It.IsAny<string>()))
            .Callback((LayerSettingsViewModel model, string _) => model.Zones = input).ReturnsAsync(true);
        var driver = new LayerSiteSettingsDisplayDriver(new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            Authorize(global::OrchardCore.Layers.Permissions.ManageLayers));
        var settings = new LayerSettings { Zones = ["Old"] };
        await driver.UpdateAsync(new SiteSettings(), settings, new UpdateEditorContext(new Shape(), "zones", false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        var site = new SiteSettings { SiteSalt = "private-site-salt", SiteName = "Unchanged" };
        site.Put(nameof(LayerSettings), new LayerSettings { Zones = ["Old"] });
        var fixture = CreateProvider(site);
        var patch = new JsonObject { ["zones"] = input is null ? new JsonArray() : new JsonArray(input) };
        var result = await fixture.Provider.UpdateAsync(patch);
        Assert.True(result.Changed);
        Assert.False(result.ReloadRequested);
        Assert.Equal(settings.Zones, result.Section.Values["zones"].AsArray().Select(value => value.GetValue<string>()));
        var expected = string.IsNullOrEmpty(input) ? Array.Empty<string>() : ["Header", "Footer", "Header", "header"];
        Assert.Equal(expected, settings.Zones);
        Assert.Equal("Unchanged", site.SiteName);
        Assert.DoesNotContain("private-site-salt", result.Section.Values.ToJsonString(), StringComparison.Ordinal);
        Assert.False((await fixture.Provider.UpdateAsync(patch)).Changed);
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
    }

    [Fact]
    public void ExistingNullList_ExplicitEmptyEdit_RestoresAnEmptyArray()
    {
        var settings = new LayerSettings { Zones = null };
        Assert.True(LayerSettingsEditor.Apply(settings, [string.Empty]));
        Assert.Empty(settings.Zones);
        Assert.False(LayerSettingsEditor.Apply(settings, []));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"zones\":null}")]
    [InlineData("{\"zones\":\"Header\"}")]
    [InlineData("{\"zones\":[null]}")]
    [InlineData("{\"zones\":[1]}")]
    [InlineData("{\"zones\":[\"Header\"],\"siteSalt\":\"unknown\"}")]
    public async Task InvalidUpdates_DoNotChangeOrSaveSettings(string json)
    {
        var site = new SiteSettings();
        site.Put(nameof(LayerSettings), new LayerSettings { Zones = ["Existing"] });
        var fixture = CreateProvider(site);
        Assert.NotEmpty((await fixture.Provider.UpdateAsync(JsonNode.Parse(json)?.AsObject())).Errors);
        Assert.Equal(["Existing"], site.As<LayerSettings>().Zones);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task SectionOperations_RequireRemoteAccessAndManageLayers()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var fixture = CreateProvider(new SiteSettings());
        ISiteSettingsSectionProvider[] providers = [fixture.Provider];
        foreach (var authorization in new[]
        {
            Authorize(RemoteManagementPermissions.AccessRemoteManagement),
            Authorize(global::OrchardCore.Layers.Permissions.ManageLayers),
        })
        {
            foreach (var result in new[]
            {
                await SiteSettingsSectionEndpoints.GetAsync(http, authorization, providers, "layer-zones"),
                await SiteSettingsSectionEndpoints.SchemaAsync(http, authorization, providers, "layer-zones"),
                await SiteSettingsSectionEndpoints.UpdateAsync(http, authorization, providers, "layer-zones", []),
            })
            {
                Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            }
        }
        fixture.Site.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
        var authorized = Authorize(RemoteManagementPermissions.AccessRemoteManagement, global::OrchardCore.Layers.Permissions.ManageLayers);
        Assert.IsType<Ok<SiteSettingsSectionUpdateResult>>(await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized, providers,
            "layer-zones", new JsonObject { ["zones"] = new JsonArray("Header") }));
    }

    [Fact]
    public async Task ZoneSettings_PersistForExistingWidgetServiceAndDisappearWithFeature()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "layer-zones");
            Assert.False(provider.Descriptor.RequiresReload);
            Assert.True((await provider.UpdateAsync(new JsonObject { ["zones"] = new JsonArray("Header", "Footer") })).Changed);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Equal(["Header", "Footer"], await scope.ServiceProvider.GetRequiredService<ILayerWidgetService>().GetZonesAsync());
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Layers");
            await manager.DisableFeaturesAsync([feature], force: true);
        });
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "layer-zones");
            return Task.CompletedTask;
        });
    }

    private static (LayerSettingsSectionProvider Provider, Mock<ISiteService> Site) CreateProvider(SiteSettings site)
    {
        var service = new Mock<ISiteService>();
        service.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        return (new LayerSettingsSectionProvider(service.Object,
            new StringLocalizer<LayerSettingsSectionProvider>(new NullStringLocalizerFactory())), service);
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
