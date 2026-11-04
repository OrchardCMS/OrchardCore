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
using OrchardCore.RemoteManagement;
using OrchardCore.Localization;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Tests.Apis.Context;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentLocalization.Controllers;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ContentCultureSettingsSectionTests
{
    [Fact]
    public async Task PartialUpdates_PreserveDefaultsAndOtherSettings_AndRetryWithoutSaving()
    {
        var site = new SiteSettings { SiteSalt = "private-salt", SiteName = "Unchanged" };
        site.Properties["Other"] = new JsonObject { ["value"] = 42 };
        var fixture = CreateProvider(site);
        var defaults = (await fixture.Provider.GetAsync()).Values;
        Assert.True(defaults["setCookie"].GetValue<bool>());
        Assert.False(defaults["redirectToHomepage"].GetValue<bool>());
        Assert.False(defaults["setCookieOnContentRequest"].GetValue<bool>());
        var update = new JsonObject { ["redirectToHomepage"] = true, ["setCookieOnContentRequest"] = true };
        var result = await fixture.Provider.UpdateAsync(update);
        Assert.True(result.Changed);
        Assert.False(result.ReloadRequested);
        Assert.True(result.Section.Values["setCookie"].GetValue<bool>());
        Assert.Equal("Unchanged", site.SiteName);
        Assert.Equal(42, site.Properties["Other"]["value"].GetValue<int>());
        Assert.DoesNotContain("private-salt", result.Section.Values.ToJsonString(), StringComparison.Ordinal);
        Assert.True(site.GetOrCreate<ContentCulturePickerSettings>().RedirectToHomepage, site.Properties[nameof(ContentCulturePickerSettings)]?.ToJsonString());
        Assert.False((await fixture.Provider.UpdateAsync(update)).Changed);
        Assert.False((await fixture.Provider.UpdateAsync([])).Changed);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
        Assert.True(site.GetOrCreate<ContentRequestCultureProviderSettings>().SetCookie);
        Assert.True(site.GetOrCreate<ContentCulturePickerSettings>().RedirectToHomepage);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"setCookie\":null}")]
    [InlineData("{\"redirectToHomepage\":null}")]
    [InlineData("{\"setCookieOnContentRequest\":null}")]
    [InlineData("{\"setCookie\":1}")]
    [InlineData("{\"setCookieOnContentRequest\":\"true\"}")]
    [InlineData("{\"setCookie\":false,\"cookieLifeTime\":10}")]
    public async Task InvalidUpdates_DoNotMutateOrSave(string json)
    {
        var site = new SiteSettings();
        var fixture = CreateProvider(site);
        Assert.NotEmpty((await fixture.Provider.UpdateAsync(JsonNode.Parse(json)?.AsObject())).Errors);
        Assert.Empty(site.Properties);
        fixture.Site.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task ExistingEditors_ApplyTheirOwnFields_AndPreserveUnboundValues()
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(model => model.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<ContentCulturePickerSettings>(), It.IsAny<string>()))
            .Callback((ContentCulturePickerSettings model, string _) => model.SetCookie = false).ReturnsAsync(true);
        updater.Setup(model => model.TryUpdateModelAsync(It.IsAny<ContentRequestCultureProviderSettings>(), It.IsAny<string>()))
            .Callback((ContentRequestCultureProviderSettings model, string _) => model.SetCookie = true).ReturnsAsync(true);
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var authorization = Authorize(ContentLocalizationPermissions.ManageContentCulturePicker);
        var picker = new ContentCulturePickerSettings { RedirectToHomepage = true };
        var request = new ContentRequestCultureProviderSettings();
        var pickerDriver = new ContentCulturePickerSettingsDriver(accessor, authorization);
        var requestDriver = new ContentRequestCultureProviderSettingsDriver(accessor, authorization);
        await pickerDriver.UpdateAsync(new SiteSettings(), picker, EditorContext(ContentCulturePickerSettingsDriver.GroupId));
        await requestDriver.UpdateAsync(new SiteSettings(), request, EditorContext(ContentRequestCultureProviderSettingsDriver.GroupId));
        Assert.False(picker.SetCookie);
        Assert.True(picker.RedirectToHomepage);
        Assert.True(request.SetCookie);
        var fixture = CreateProvider(new SiteSettings());
        var result = await fixture.Provider.UpdateAsync(new JsonObject { ["setCookie"] = false, ["redirectToHomepage"] = true, ["setCookieOnContentRequest"] = true });
        Assert.Equal(picker.SetCookie, result.Section.Values["setCookie"].GetValue<bool>());
        Assert.Equal(picker.RedirectToHomepage, result.Section.Values["redirectToHomepage"].GetValue<bool>());
        Assert.Equal(request.SetCookie, result.Section.Values["setCookieOnContentRequest"].GetValue<bool>());

        UpdateEditorContext EditorContext(string group) => new(new Shape(), group, false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task UpdatedSettings_ControlExistingPickerAndRequestCulturePaths(bool cookie, bool homepage)
    {
        var fixture = CreateProvider(new SiteSettings());
        await fixture.Provider.UpdateAsync(new JsonObject { ["setCookie"] = cookie, ["redirectToHomepage"] = homepage, ["setCookieOnContentRequest"] = cookie });
        var picker = new Mock<IContentCulturePickerService>();
        picker.Setup(service => service.GetLocalizationsFromRouteAsync(new PathString("/missing"))).ReturnsAsync([]);
        picker.Setup(service => service.GetLocalizationsFromRouteAsync(new PathString("/")))
            .ReturnsAsync([new LocalizationEntry { Culture = "fr", ContentItemId = "french-home" }]);
        picker.Setup(service => service.GetLocalizationFromRouteAsync(It.IsAny<PathString>()))
            .ReturnsAsync(new LocalizationEntry { Culture = "fr", ContentItemId = "french-page" });
        var localization = new Mock<ILocalizationService>();
        localization.Setup(service => service.GetSupportedCulturesAsync()).ReturnsAsync(["en", "fr"]);
        var url = new Mock<IUrlHelper>();
        url.Setup(helper => helper.Action(It.IsAny<UrlActionContext>())).Returns("/tenant/french-home");
        using var services = new ServiceCollection().AddSingleton(fixture.Site.Object).AddSingleton(picker.Object).BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.PathBase = "/tenant";
        http.Request.Path = "/french-page";
        var controller = new ContentCulturePickerController(fixture.Site.Object, localization.Object, picker.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = http }, Url = url.Object,
        };
        var result = Assert.IsType<LocalRedirectResult>(await controller.RedirectToLocalizedContent("fr", "/missing", "?page=2"));
        Assert.Equal(homepage ? "/tenant/french-home?page=2" : "/tenant/missing?page=2", result.Url);
        Assert.False(result.Permanent);
        picker.Verify(service => service.SetContentCulturePickerCookie("fr"), cookie ? Times.Once : Times.Never);
        picker.Verify(service => service.GetLocalizationsFromRouteAsync(new PathString("/")), homepage ? Times.Once : Times.Never);
        var culture = await new ContentRequestCultureProvider().DetermineProviderCultureResult(http);
        Assert.Equal("fr", Assert.Single(culture.Cultures).Value);
        picker.Verify(service => service.SetContentCulturePickerCookie("fr"), cookie ? Times.Exactly(2) : Times.Never());
    }

    [Fact]
    public async Task SectionOperations_RequireRemoteAccessAndPickerPermission()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var fixture = CreateProvider(new SiteSettings());
        ISiteSettingsSectionProvider[] providers = [fixture.Provider];
        foreach (var authorization in new[] { Authorize(RemoteManagementPermissions.AccessRemoteManagement), Authorize(ContentLocalizationPermissions.ManageContentCulturePicker) })
        {
            foreach (var result in new[]
            {
                await SiteSettingsSectionEndpoints.GetAsync(http, authorization, providers, "content-culture-picker"),
                await SiteSettingsSectionEndpoints.SchemaAsync(http, authorization, providers, "content-culture-picker"),
                await SiteSettingsSectionEndpoints.UpdateAsync(http, authorization, providers, "content-culture-picker", []),
            })
            {
                Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            }
        }
        fixture.Site.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task Settings_PersistAndDisappearWithPickerFeature()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await SetFeatureAsync(context, true);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "content-culture-picker");
            Assert.False(provider.Descriptor.RequiresReload);
            Assert.True((await provider.UpdateAsync(new JsonObject { ["setCookie"] = false, ["setCookieOnContentRequest"] = true })).Changed);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var site = scope.ServiceProvider.GetRequiredService<ISiteService>();
            Assert.False((await site.GetSettingsAsync<ContentCulturePickerSettings>()).SetCookie);
            Assert.True((await site.GetSettingsAsync<ContentRequestCultureProviderSettings>()).SetCookie);
        });
        await SetFeatureAsync(context, false);
        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), provider => provider.Descriptor.Name == "content-culture-picker");
            return Task.CompletedTask;
        });
    }

    private static Task SetFeatureAsync(SiteContext context, bool enabled) => context.UsingTenantScopeAsync(async scope =>
    {
        var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
        var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.ContentLocalization.ContentCulturePicker");
        if (enabled) { await manager.EnableFeaturesAsync([feature], force: true); }
        else { await manager.DisableFeaturesAsync([feature], force: true); }
    });

    private static (ContentCultureSettingsSectionProvider Provider, Mock<ISiteService> Site) CreateProvider(SiteSettings site)
    {
        site.IsReadOnly = false;
        var service = new Mock<ISiteService>();
        service.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);
        return (new ContentCultureSettingsSectionProvider(service.Object,
            new StringLocalizer<ContentCultureSettingsSectionProvider>(new NullStringLocalizerFactory())), service);
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
