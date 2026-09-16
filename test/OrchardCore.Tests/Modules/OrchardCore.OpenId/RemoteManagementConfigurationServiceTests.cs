using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class RemoteManagementConfigurationServiceTests
{
    [Fact]
    public async Task GetStatusAsync_MissingConfiguration_ReturnsNotReady()
    {
        var service = CreateService();
        var status = await service.GetStatusAsync();
        Assert.False(status.IsReady);
        Assert.False(status.ManagementScopeConfigured);
    }

    [Fact]
    public async Task ConfigureAsync_MissingConfiguration_ConfiguresSharedSettingsWithoutDeviceFlow()
    {
        var serverSettings = new OpenIdServerSettings
        {
            UserinfoEndpointPath = "/custom/userinfo",
            AllowPasswordFlow = true,
        };
        var validationSettings = new OpenIdValidationSettings
        {
            Authority = new Uri("https://identity.example.com"),
            MetadataAddress = new Uri("https://identity.example.com/metadata"),
            Audience = "preserved-audience",
        };
        OpenIdScopeDescriptor createdScope = null;
        var scopeManager = new Mock<IOpenIdScopeManager>();
        scopeManager.Setup(manager => manager.CreateAsync(It.IsAny<OpenIdScopeDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<OpenIddictScopeDescriptor, CancellationToken>((descriptor, _) => createdScope = (OpenIdScopeDescriptor)descriptor);
        var service = CreateService(scopeManager, serverSettings, validationSettings);

        await service.ConfigureAsync();

        Assert.Equal("/custom/userinfo", serverSettings.UserinfoEndpointPath);
        Assert.True(serverSettings.AllowPasswordFlow);
        Assert.True(serverSettings.AllowAuthorizationCodeFlow);
        Assert.True(serverSettings.AllowClientCredentialsFlow);
        Assert.False(serverSettings.AllowDeviceAuthorizationFlow);
        Assert.True(serverSettings.AllowRefreshTokenFlow);
        Assert.True(serverSettings.RequireProofKeyForCodeExchange);
        Assert.Equal("TestTenant", validationSettings.Tenant);
        Assert.Null(validationSettings.Authority);
        Assert.Null(validationSettings.MetadataAddress);
        Assert.Equal("preserved-audience", validationSettings.Audience);
        Assert.Contains("orchardcore", createdScope.Resources);
    }

    [Fact]
    public async Task ConfigureAsync_ExistingConfiguration_PreservesAdditionalResourcesAndDeviceFlow()
    {
        var existingScope = new object();
        OpenIdScopeDescriptor updatedScope = null;
        var scopeManager = new Mock<IOpenIdScopeManager>();
        scopeManager.Setup(manager => manager.FindByNameAsync("orchardcore.management", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScope);
        scopeManager.Setup(manager => manager.PopulateAsync(existingScope, It.IsAny<OpenIdScopeDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictScopeDescriptor, CancellationToken>((_, descriptor, _) => descriptor.Resources.Add("other-resource"));
        scopeManager.Setup(manager => manager.UpdateAsync(existingScope, It.IsAny<OpenIdScopeDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictScopeDescriptor, CancellationToken>((_, descriptor, _) => updatedScope = (OpenIdScopeDescriptor)descriptor);
        var settings = new OpenIdServerSettings { AllowDeviceAuthorizationFlow = true, DeviceAuthorizationEndpointPath = "/custom/device" };
        var service = CreateService(scopeManager, settings);

        await service.ConfigureAsync();

        Assert.Contains("other-resource", updatedScope.Resources);
        Assert.Contains("orchardcore", updatedScope.Resources);
        Assert.True(settings.AllowDeviceAuthorizationFlow);
        Assert.Equal("/custom/device", settings.DeviceAuthorizationEndpointPath);
    }

    [Fact]
    public async Task GetStatusAsync_SharedSettingsConfigured_IsReadyWithoutClientRegistration()
    {
        var scope = new object();
        var manager = new Mock<IOpenIdScopeManager>();
        manager.Setup(value => value.FindByNameAsync("orchardcore.management", It.IsAny<CancellationToken>())).ReturnsAsync(scope);
        manager.Setup(value => value.GetResourcesAsync(scope, It.IsAny<CancellationToken>())).ReturnsAsync(ImmutableArray.Create("orchardcore"));
        var service = CreateService(manager);
        await service.ConfigureAsync();
        Assert.True((await service.GetStatusAsync()).IsReady);
    }

    private static RemoteManagementConfigurationService CreateService(
        Mock<IOpenIdScopeManager> scopeManager = null,
        OpenIdServerSettings serverSettings = null,
        OpenIdValidationSettings validationSettings = null)
    {
        scopeManager ??= new Mock<IOpenIdScopeManager>();
        serverSettings ??= new OpenIdServerSettings();
        validationSettings ??= new OpenIdValidationSettings();
        var serverService = new Mock<IOpenIdServerService>();
        var validationService = new Mock<IOpenIdValidationService>();
        serverService.Setup(service => service.GetSettingsAsync()).ReturnsAsync(serverSettings);
        serverService.Setup(service => service.LoadSettingsAsync()).ReturnsAsync(serverSettings);
        serverService.Setup(service => service.ValidateSettingsAsync(serverSettings)).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        validationService.Setup(service => service.GetSettingsAsync()).ReturnsAsync(validationSettings);
        validationService.Setup(service => service.LoadSettingsAsync()).ReturnsAsync(validationSettings);
        validationService.Setup(service => service.ValidateSettingsAsync(validationSettings)).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        return new RemoteManagementConfigurationService(scopeManager.Object, serverService.Object, validationService.Object,
            new ShellSettings { Name = "TestTenant" });
    }
}
