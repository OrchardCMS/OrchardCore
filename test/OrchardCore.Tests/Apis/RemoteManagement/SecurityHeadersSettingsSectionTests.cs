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
using Microsoft.Extensions.Options;
using OrchardCore.Security.Drivers;
using OrchardCore.Security.Services;
using OrchardCore.Security.Settings;
using OrchardCore.Security.ViewModels;
using OrchardCore.RemoteManagement;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SecurityHeadersSettingsSectionTests
{
    [Theory]
    [InlineData("strict-origin", true)]
    [InlineData("invalid", false)]
    [InlineData("origin\r\nInjected: value", false)]
    public async Task ExistingEditor_ValidatesBeforeMutationAndOnlyReloadsOnChange(string referrer, bool valid)
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(model => model.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<SecuritySettingsViewModel>(), It.IsAny<string>()))
            .Callback((SecuritySettingsViewModel model, string _) => { model.ReferrerPolicy = referrer; })
            .ReturnsAsync(true);
        var release = new Mock<IShellReleaseManager>();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var options = new Mock<IOptionsSnapshot<SecuritySettings>>();
        options.SetupGet(value => value.Value).Returns(new SecuritySettings());
        var driver = new SecuritySettingsDisplayDriver(release.Object, accessor,
            Authorize(SecurityPermissions.ManageSecurityHeadersSettings), options.Object, Mock.Of<INotifier>(),
            Mock.Of<IHtmlLocalizer<SecuritySettingsDisplayDriver>>(),
            new StringLocalizer<SecuritySettingsDisplayDriver>(new NullStringLocalizerFactory()));
        var context = new UpdateEditorContext(new Shape(), "SecurityHeaders", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var settings = new SecuritySettings();
        await driver.UpdateAsync(new SiteSettings(), settings, context);
        Assert.Equal(valid ? referrer : "no-referrer", settings.ReferrerPolicy);
        release.Verify(manager => manager.RequestRelease(), valid ? Times.Once : Times.Never);
        if (valid)
        {
            await driver.UpdateAsync(new SiteSettings(), settings, context);
            release.Verify(manager => manager.RequestRelease(), Times.Once);
        }
        else
        {
            Assert.False(updater.Object.ModelState.IsValid);
        }
    }

    [Fact]
    public async Task PartialUpdate_UsesReplacementAndNormalizationWithoutChangingOtherSettings()
    {
        var site = new SiteSettings { IsReadOnly = false, SiteName = "Unchanged", SiteSalt = "private" };
        var fixture = CreateProvider(site);
        var patch = JsonNode.Parse("""{"contentSecurityPolicy":{"default-src":"'self'","sandbox":null,"upgrade-insecure-requests":"ignored"},"permissionsPolicy":{"camera":"self","microphone":"()"},"referrerPolicy":"strict-origin"}""").AsObject();
        var first = await fixture.Provider.UpdateAsync(patch);
        Assert.Empty(first.Errors);
        Assert.True(first.Changed);
        Assert.Null(first.Section.Values["contentSecurityPolicy"]["sandbox"]);
        Assert.Null(first.Section.Values["contentSecurityPolicy"]["upgrade-insecure-requests"]);
        Assert.False(first.Section.Values["permissionsPolicy"].AsObject().ContainsKey("microphone"));
        var retry = await fixture.Provider.UpdateAsync(patch);
        Assert.False(retry.Changed);
        Assert.True(JsonNode.DeepEquals(first.Section.Values, retry.Section.Values));
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
        var cleared = await fixture.Provider.UpdateAsync(JsonNode.Parse("""{"contentSecurityPolicy":{}}""").AsObject());
        Assert.Empty(cleared.Section.Values["contentSecurityPolicy"].AsObject());
        Assert.Equal("self", cleared.Section.Values["permissionsPolicy"]["camera"].GetValue<string>());
        Assert.Equal("strict-origin", cleared.Section.Values["referrerPolicy"].GetValue<string>());
        Assert.Equal("Unchanged", site.SiteName);
        Assert.Equal("private", site.SiteSalt);
        Assert.DoesNotContain("private", first.Section.Values.ToJsonString(), StringComparison.Ordinal);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Exactly(2));
        fixture.Release.Verify(service => service.RequestRelease(), Times.Exactly(2));
    }

    [Theory]
    [InlineData("{\"contentSecurityPolicy\":null}")]
    [InlineData("{\"permissionsPolicy\":[]}")]
    [InlineData("{\"permissionsPolicy\":{\"camera\":null}}")]
    [InlineData("{\"permissionsPolicy\":{\"camera\":false}}")]
    [InlineData("{\"contentSecurityPolicy\":{\"default-src\":3}}")]
    [InlineData("{\"referrerPolicy\":null}")]
    [InlineData("{\"referrerPolicy\":\"invalid\"}")]
    [InlineData("{\"contentSecurityPolicy\":{\"default-src\":\"self; script-src *\"}}")]
    [InlineData("{\"permissionsPolicy\":{\"camera\":\"self, microphone=*\"}}")]
    [InlineData("{\"contentSecurityPolicy\":{\"bad name\":\"self\"}}")]
    [InlineData("{\"siteSalt\":\"private\"}")]
    [InlineData("{\"contentTypeOptions\":\"disabled\"}")]
    public async Task InvalidUpdate_DoesNotSaveOrReload(string json)
    {
        var fixture = CreateProvider(new SiteSettings { IsReadOnly = false });
        var result = await fixture.Provider.UpdateAsync(JsonNode.Parse(json).AsObject());
        Assert.NotEmpty(result.Errors);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task ConfiguredSettings_AreReadOnlyAndRejectWritesBeforeLoadingTenantData()
    {
        var settings = new SecuritySettings { FromConfiguration = true, ReferrerPolicy = "same-origin" };
        var fixture = CreateProvider(new SiteSettings(), settings);
        var response = await fixture.Provider.GetAsync();
        Assert.True(response.IsReadOnly);
        Assert.Equal("configuration", response.Source);
        Assert.Equal("same-origin", response.Values["referrerPolicy"].GetValue<string>());
        Assert.NotEmpty((await fixture.Provider.UpdateAsync([])).Errors);
        fixture.Site.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task SecurityHeaders_PersistAndDisappearWithTheirFeature()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetFeatureAsync(context, true);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "security-headers");
            var result = await provider.UpdateAsync(new JsonObject { ["referrerPolicy"] = "strict-origin" });
            Assert.True(result.Changed);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "security-headers");
            Assert.Equal("strict-origin", (await provider.GetAsync()).Values["referrerPolicy"].GetValue<string>());
        });
        await SetFeatureAsync(context, false);
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "security-headers");
            Assert.Contains(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "layer-zones");
            return Task.CompletedTask;
        });
    }

    private static Task SetFeatureAsync(SiteContext context, bool enabled) => context.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.Security");
        if (enabled)
        {
            await manager.EnableFeaturesAsync([feature], force: true);
        }
        else
        {
            await manager.DisableFeaturesAsync([feature], force: true);
        }
    });

    private static (SecuritySettingsSectionProvider Provider, Mock<ISiteService> Site, Mock<IShellReleaseManager> Release)
        CreateProvider(SiteSettings site, SecuritySettings options = null)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var release = new Mock<IShellReleaseManager>();
        var snapshot = new Mock<IOptionsSnapshot<SecuritySettings>>();
        snapshot.SetupGet(value => value.Value).Returns(() => options ?? site.GetOrCreate<SecuritySettings>());
        return (new SecuritySettingsSectionProvider(siteService.Object, snapshot.Object, release.Object,
            new StringLocalizer<SecuritySettingsSectionProvider>(new NullStringLocalizerFactory())), siteService, release);
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
