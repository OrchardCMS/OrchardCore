using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules.Manifest;
using OrchardCore.RemoteManagement;
using OrchardCore.Themes.Endpoints.Management;
using OrchardCore.Themes.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Themes;

public class ThemeManagementEndpointsTests
{
    [Fact]
    public async Task CapabilityProvider_AdvertisesThemeManagement()
    {
        var capabilities = await new ThemesRemoteManagementCapabilityProvider().GetCapabilitiesAsync();
        var capability = Assert.Single(capabilities);

        Assert.Equal(ThemeManagementEndpointConventions.CapabilityName, capability.Id);
        Assert.Equal("Themes", capability.DisplayName);
        Assert.Equal(
            $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
            capability.Version);
    }

    [Fact]
    public void ToResponse_AdminCurrentTheme_MapsState()
    {
        var feature = CreateThemeFeature("TheAdmin", [ManifestConstants.AdminTag]);

        var response = ThemeManagementEndpoints.ToResponse(
            feature,
            isEnabled: true,
            currentSiteTheme: "TheTheme",
            currentAdminTheme: "TheAdmin");

        Assert.Equal("TheAdmin", response.Id);
        Assert.True(response.IsAdmin);
        Assert.True(response.IsEnabled);
        Assert.True(response.IsCurrent);
    }

    [Fact]
    public void IsManageableTheme_HiddenTheme_ReturnsFalse()
    {
        var feature = CreateThemeFeature("HiddenTheme", ["hidden"]);

        Assert.False(ThemeManagementEndpoints.IsManageableTheme(feature));
    }

    [Fact]
    public async Task EnableAsync_EnabledTheme_DoesNotEnableAgain()
    {
        var feature = CreateThemeFeature("TheTheme", []);
        var shellFeaturesManager = new Mock<IShellFeaturesManager>();
        shellFeaturesManager.Setup(manager => manager.GetAvailableFeaturesAsync()).ReturnsAsync([feature]);
        shellFeaturesManager.Setup(manager => manager.GetEnabledFeaturesAsync()).ReturnsAsync([feature]);

        var result = await ThemeManagementEndpoints.EnableAsync(
            new DefaultHttpContext(),
            "thetheme",
            CreateAuthorizationService().Object,
            shellFeaturesManager.Object,
            Mock.Of<ISiteThemeService>(service => service.GetSiteThemeNameAsync() == Task.FromResult("TheTheme")),
            Mock.Of<IAdminThemeService>(service => service.GetAdminThemeNameAsync() == Task.FromResult("TheAdmin")),
            Mock.Of<IStringLocalizer<global::OrchardCore.Themes.Permissions>>());

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        shellFeaturesManager.Verify(
            manager => manager.UpdateFeaturesAsync(
                It.IsAny<IEnumerable<IFeatureInfo>>(),
                It.IsAny<IEnumerable<IFeatureInfo>>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task SetCurrentAsync_DisabledSiteTheme_SelectsAndEnablesTheme()
    {
        var feature = CreateThemeFeature("TheTheme", []);
        var shellFeaturesManager = new Mock<IShellFeaturesManager>();
        shellFeaturesManager.Setup(manager => manager.GetAvailableFeaturesAsync()).ReturnsAsync([feature]);
        shellFeaturesManager
            .Setup(manager => manager.GetEnabledFeaturesAsync())
            .ReturnsAsync([]);
        shellFeaturesManager
            .Setup(manager => manager.UpdateFeaturesAsync(
                It.Is<IEnumerable<IFeatureInfo>>(features => !features.Any()),
                It.Is<IEnumerable<IFeatureInfo>>(features => features.Single() == feature),
                true))
            .ReturnsAsync(([], [feature]));
        var siteThemeService = new Mock<ISiteThemeService>();
        siteThemeService.Setup(service => service.GetSiteThemeNameAsync()).ReturnsAsync("PreviousTheme");
        var adminThemeService = Mock.Of<IAdminThemeService>(service =>
            service.GetAdminThemeNameAsync() == Task.FromResult("TheAdmin"));
        var freshSiteThemeService = new Mock<ISiteThemeService>();
        var shellSettings = new ShellSettings { Name = "Default" };
        var shellContext = new ShellContext
        {
            Settings = shellSettings,
            ServiceProvider = new ServiceCollection()
                .AddSingleton(freshSiteThemeService.Object)
                .AddSingleton(adminThemeService)
                .BuildServiceProvider(),
        };
        var shellHost = new Mock<IShellHost>();
        shellHost
            .Setup(host => host.GetScopeAsync(shellSettings))
            .Returns(shellContext.CreateScopeAsync);

        var result = await ThemeManagementEndpoints.SetCurrentAsync(
            new DefaultHttpContext(),
            "thetheme",
            CreateAuthorizationService().Object,
            shellFeaturesManager.Object,
            siteThemeService.Object,
            adminThemeService,
            shellHost.Object,
            shellSettings,
            Mock.Of<IStringLocalizer<global::OrchardCore.Themes.Permissions>>());

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        siteThemeService.Verify(service => service.SetSiteThemeAsync(It.IsAny<string>()), Times.Never);
        freshSiteThemeService.Verify(service => service.SetSiteThemeAsync("TheTheme"), Times.Once);
        shellFeaturesManager.Verify(
            manager => manager.UpdateFeaturesAsync(
                It.Is<IEnumerable<IFeatureInfo>>(features => !features.Any()),
                It.Is<IEnumerable<IFeatureInfo>>(features => features.Single() == feature),
                true),
            Times.Once);
    }

    [Fact]
    public async Task SetCurrentAsync_EnablementFails_DoesNotChangeSelection()
    {
        var feature = CreateThemeFeature("TheTheme", []);
        var shellFeaturesManager = new Mock<IShellFeaturesManager>();
        shellFeaturesManager.Setup(manager => manager.GetAvailableFeaturesAsync()).ReturnsAsync([feature]);
        shellFeaturesManager.Setup(manager => manager.GetEnabledFeaturesAsync()).ReturnsAsync([]);
        shellFeaturesManager
            .Setup(manager => manager.UpdateFeaturesAsync(
                It.IsAny<IEnumerable<IFeatureInfo>>(),
                It.IsAny<IEnumerable<IFeatureInfo>>(),
                true))
            .ReturnsAsync(([], []));
        var siteThemeService = new Mock<ISiteThemeService>();
        siteThemeService.Setup(service => service.GetSiteThemeNameAsync()).ReturnsAsync("PreviousTheme");

        var result = await ThemeManagementEndpoints.SetCurrentAsync(
            new DefaultHttpContext(),
            "TheTheme",
            CreateAuthorizationService().Object,
            shellFeaturesManager.Object,
            siteThemeService.Object,
            Mock.Of<IAdminThemeService>(service => service.GetAdminThemeNameAsync() == Task.FromResult("TheAdmin")),
            Mock.Of<IShellHost>(),
            new ShellSettings { Name = "Default" },
            Mock.Of<IStringLocalizer<global::OrchardCore.Themes.Permissions>>());

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        siteThemeService.Verify(service => service.SetSiteThemeAsync(It.IsAny<string>()), Times.Never);
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService;
    }

    private static IFeatureInfo CreateThemeFeature(string id, string[] tags)
    {
        var manifest = Mock.Of<IManifestInfo>(candidate =>
            candidate.Tags == tags &&
            candidate.Name == id);
        var extension = Mock.Of<IThemeExtensionInfo>(candidate =>
            candidate.Id == id &&
            candidate.Manifest == manifest);

        return Mock.Of<IFeatureInfo>(candidate =>
            candidate.Id == id &&
            candidate.Name == id &&
            candidate.Extension == extension);
    }
}
