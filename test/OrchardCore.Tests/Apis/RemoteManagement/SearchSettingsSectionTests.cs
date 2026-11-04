using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
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
using OrchardCore.Search;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Localization;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Models;
using OrchardCore.Search.Services;
using OrchardCore.Search.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SearchSettingsSectionTests
{
    [Theory]
    [InlineData("articles")]
    [InlineData(null)]
    public async Task AdminAndApi_ApplySameSelectionAndText(string selection)
    {
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(value => value.FindByNameAsync("articles")).ReturnsAsync(new IndexProfile { Name = "Articles" });
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<SearchSettingsViewModel>(), It.IsAny<string>()))
            .Callback((SearchSettingsViewModel model, string _) =>
            {
                model.DefaultIndexProfileName = selection; model.PageTitle = "Find articles"; model.Placeholder = "Search here";
            }).ReturnsAsync(true);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var driver = new SearchSettingsDisplayDriver(new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            authorization.Object, profiles.Object, Localizer<SearchSettingsDisplayDriver>());
        var settings = new SearchSettings { DefaultIndexProfileName = "Old" };
        await driver.UpdateAsync(new SiteSettings(), settings, new UpdateEditorContext(new Shape(), "search", false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        Assert.True(updater.Object.ModelState.IsValid);
        var site = new SiteSettings { SiteName = "Keep" };
        site.Put(nameof(SearchSettings), new SearchSettings { DefaultIndexProfileName = "Old" });
        var (provider, service) = Create(site, profiles.Object);
        var patch = new JsonObject { ["defaultIndexProfileName"] = selection, ["pageTitle"] = "Find articles", ["placeholder"] = "Search here" };

        var result = await provider.UpdateAsync(patch);

        Assert.Empty(result.Errors);
        Assert.True(result.Changed);
        Assert.Equal(settings.DefaultIndexProfileName, result.Section.Values["defaultIndexProfileName"]?.GetValue<string>());
        Assert.Equal(settings.PageTitle, result.Section.Values["pageTitle"].GetValue<string>());
        Assert.Equal(settings.Placeholder, result.Section.Values["placeholder"].GetValue<string>());
        Assert.False((await provider.UpdateAsync(patch)).Changed);
        Assert.Equal("Keep", site.SiteName);
        service.Verify(value => value.UpdateSiteSettingsAsync(site), Times.Once());
    }

    [Theory]
    [InlineData("{\"pageTitle\":true}")]
    [InlineData("{\"providerName\":\"Lucene\"}")]
    [InlineData("{\"defaultIndexProfileName\":\"missing\",\"pageTitle\":\"Changed\"}")]
    public async Task InvalidPatch_DoesNotSaveOrPartiallyChangeSettings(string json)
    {
        var site = new SiteSettings();
        site.Put(nameof(SearchSettings), new SearchSettings { DefaultIndexProfileName = "Old", PageTitle = "Original" });
        var (provider, service) = Create(site, Mock.Of<IIndexProfileStore>());

        var result = await provider.UpdateAsync(JsonNode.Parse(json).AsObject());

        Assert.NotEmpty(result.Errors);
        Assert.Equal("Original", site.GetOrCreate<SearchSettings>().PageTitle);
        Assert.Equal("Old", site.GetOrCreate<SearchSettings>().DefaultIndexProfileName);
        service.Verify(value => value.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("")]
    public async Task OmittedDefault_PreservesStaleOrEmptyReference(string existing)
    {
        var site = new SiteSettings();
        site.Put(nameof(SearchSettings), new SearchSettings { DefaultIndexProfileName = existing, Placeholder = "Keep" });
        var profiles = new Mock<IIndexProfileStore>(MockBehavior.Strict);
        var (provider, service) = Create(site, profiles.Object);
        Assert.False((await provider.UpdateAsync([])).Changed);
        var result = await provider.UpdateAsync(new JsonObject { ["pageTitle"] = "Changed" });
        Assert.Equal(existing, result.Section.Values["defaultIndexProfileName"].GetValue<string>());
        Assert.Equal("Keep", result.Section.Values["placeholder"].GetValue<string>());
        profiles.VerifyNoOtherCalls();
        service.Verify(value => value.UpdateSiteSettingsAsync(site), Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdminPartialSubmission_PreservesOmittedFieldsAndRejectsInvalidSelection(bool invalidSelection)
    {
        var profiles = new Mock<IIndexProfileStore>();
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<SearchSettingsViewModel>(), It.IsAny<string>()))
            .Callback((SearchSettingsViewModel model, string _) =>
            {
                model.PageTitle = "Changed";
                if (invalidSelection) { model.DefaultIndexProfileName = "missing"; }
            }).ReturnsAsync(true);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var driver = new SearchSettingsDisplayDriver(new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            authorization.Object, profiles.Object, Localizer<SearchSettingsDisplayDriver>());
        var settings = new SearchSettings { DefaultIndexProfileName = "Old", Placeholder = "Keep", PageTitle = "Original" };

        await driver.UpdateAsync(new SiteSettings(), settings, new UpdateEditorContext(new Shape(), "search", false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));

        Assert.Equal(!invalidSelection, updater.Object.ModelState.IsValid);
        Assert.Equal("Old", settings.DefaultIndexProfileName);
        Assert.Equal("Keep", settings.Placeholder);
        Assert.Equal(invalidSelection ? "Original" : "Changed", settings.PageTitle);
    }

    [Fact]
    public async Task SectionOperations_RequireBothManagementAndSearchPermission()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var (provider, site) = Create(new SiteSettings(), Mock.Of<IIndexProfileStore>());
        ISiteSettingsSectionProvider[] providers = [provider];
        foreach (var authorization in new[]
        {
            Authorize(RemoteManagementPermissions.AccessRemoteManagement),
            Authorize(SearchPermissions.ManageSearchSettings),
            Authorize(RemoteManagementPermissions.AccessRemoteManagement, IndexingPermissions.ManageIndexes),
        })
        {
            foreach (var result in new[]
            {
                await SiteSettingsSectionEndpoints.GetAsync(http, authorization, providers, "frontend-search"),
                await SiteSettingsSectionEndpoints.SchemaAsync(http, authorization, providers, "frontend-search"),
                await SiteSettingsSectionEndpoints.UpdateAsync(http, authorization, providers, "frontend-search", []),
            })
            {
                Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
            }
        }
        site.VerifyNoOtherCalls();
        var authorized = Authorize(RemoteManagementPermissions.AccessRemoteManagement, SearchPermissions.ManageSearchSettings);
        Assert.IsType<Ok<SiteSettingsSectionUpdateResult>>(await SiteSettingsSectionEndpoints.UpdateAsync(http, authorized, providers,
            "frontend-search", new JsonObject { ["pageTitle"] = "Allowed" }));
    }

    [Fact]
    public async Task Admin_Denied_DoesNotBindOrChangeSettings()
    {
        var updater = new Mock<IUpdateModel>(MockBehavior.Strict);
        var profiles = new Mock<IIndexProfileStore>(MockBehavior.Strict);
        var settings = new SearchSettings { PageTitle = "Keep" };
        var driver = new SearchSettingsDisplayDriver(new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            Authorize(), profiles.Object, Localizer<SearchSettingsDisplayDriver>());

        Assert.Null(await driver.UpdateAsync(new SiteSettings(), settings, new UpdateEditorContext(new Shape(), "search", false, "",
            Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object)));

        Assert.Equal("Keep", settings.PageTitle);
        updater.VerifyNoOtherCalls();
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Section_PersistsInTenantAndFollowsFeatureLifecycle()
    {
        using var owner = new SiteContext();
        using var other = new SiteContext();
        await owner.InitializeAsync();
        await other.InitializeAsync();
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(value => value.Id == "OrchardCore.Search");
            await manager.EnableFeaturesAsync([feature], force: true);
        });
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var site = await siteService.LoadSiteSettingsAsync();
            site.Properties[nameof(SearchSettings)] = new JsonObject { ["ProviderName"] = "legacy-provider" };
            await siteService.UpdateSiteSettingsAsync(site);
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), value => value.Descriptor.Name == "frontend-search");
            var result = await provider.UpdateAsync(new JsonObject { ["pageTitle"] = "Owner search", ["placeholder"] = "Owner placeholder" });
            Assert.True(result.Changed);
            Assert.False(result.ReloadRequested);
            Assert.DoesNotContain("legacy-provider", result.Section.Values.ToJsonString(), StringComparison.Ordinal);
        });
        await other.UsingTenantScopeAsync(async scope =>
        {
            var settings = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSettingsAsync<SearchSettings>();
            Assert.NotEqual("Owner search", settings.PageTitle);
        });
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var site = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
            Assert.Equal("Owner search", site.GetOrCreate<SearchSettings>().PageTitle);
            Assert.Equal("Owner placeholder", site.GetOrCreate<SearchSettings>().Placeholder);
            Assert.Equal("legacy-provider", site.Properties[nameof(SearchSettings)]["ProviderName"].GetValue<string>());
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(value => value.Id == "OrchardCore.Search");
            await manager.DisableFeaturesAsync([feature], force: true);
        });
        await owner.UsingTenantScopeAsync(async scope =>
        {
            Assert.DoesNotContain(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), value => value.Descriptor.Name == "frontend-search");
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).Single(value => value.Id == "OrchardCore.Search");
            await manager.EnableFeaturesAsync([feature], force: true);
        });
        await owner.UsingTenantScopeAsync(async scope =>
        {
            var provider = Assert.Single(scope.ServiceProvider.GetServices<ISiteSettingsSectionProvider>(), value => value.Descriptor.Name == "frontend-search");
            Assert.Equal("Owner search", (await provider.GetAsync()).Values["pageTitle"].GetValue<string>());
        });
    }

    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }

    private static StringLocalizer<T> Localizer<T>() => new(new NullStringLocalizerFactory());

    private static (SearchSettingsSectionProvider, Mock<ISiteService>) Create(SiteSettings site, IIndexProfileStore profiles)
    {
        var service = new Mock<ISiteService>();
        service.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        return (new SearchSettingsSectionProvider(service.Object, profiles, Localizer<SearchSettingsSectionProvider>()), service);
    }
}
