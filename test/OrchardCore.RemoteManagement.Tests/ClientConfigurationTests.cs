using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Roles;
using OrchardCore.Security;

namespace OrchardCore.RemoteManagement.Tests;

public class ClientConfigurationTests
{
    [Fact]
    public async Task Provision_NewApplication_UsesOnlyClientCredentialsAndCustomAdminRole()
    {
        var manager = new Mock<IOpenIdApplicationManager>();
        var roles = new Mock<ISystemRoleProvider>();
        roles.Setup(value => value.GetAdminRole()).Returns(new Role { RoleName = "SiteOwners" });
        OpenIdApplicationDescriptor created = null;
        manager.Setup(value => value.CreateAsync(It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<OpenIddictApplicationDescriptor, CancellationToken>((value, _) => created = (OpenIdApplicationDescriptor)value);
        var credentials = RemoteManagementClientCredentials.Generate();

        await new RemoteManagementClientProvisioningService(manager.Object, roles.Object).CreateAsync(credentials);

        Assert.Equal(credentials.ClientId, created.ClientId);
        Assert.Equal(credentials.ClientSecret, created.ClientSecret);
        Assert.Equal(OpenIddictConstants.ClientTypes.Confidential, created.ClientType);
        Assert.Equal("SiteOwners", Assert.Single(created.Roles));
        Assert.Equal(3, created.Permissions.Count);
        Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, created.Permissions);
        Assert.Empty(created.RedirectUris);
        manager.Verify(value => value.FindByClientIdAsync("orchardcore-cli", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Provision_ExistingApplication_IsNeverOverwritten()
    {
        var credentials = RemoteManagementClientCredentials.Generate();
        var manager = new Mock<IOpenIdApplicationManager>();
        manager.Setup(value => value.FindByClientIdAsync(credentials.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(new object());
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new RemoteManagementClientProvisioningService(manager.Object, Mock.Of<ISystemRoleProvider>()).CreateAsync(credentials));
        manager.Verify(value => value.CreateAsync(It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()), Times.Never);
        manager.Verify(value => value.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "orchardcore-cli")]
    public async Task Manifest_CliFeature_ControlsPomiClientDiscovery(bool enabled, string clientId)
    {
        var service = new RemoteManagementManifestService([], Options.Create(new OpenIddict.Server.OpenIddictServerOptions()),
            new ShellSettings { Name = "TenantA" }, Options.Create(new RemoteManagementOptions { CliEnabled = enabled }));
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com");
        var manifest = await service.CreateAuthenticatedAsync(context.Request);
        Assert.Equal(clientId, manifest.Authentication.ClientId);
        Assert.Equal(enabled ? "1.0.0" : null, manifest.MinimumCliVersion);
    }

    [Fact]
    public async Task CliConfigure_MissingApplication_CreatesOnlyPomiAndEnablesDeviceFlow()
    {
        var settings = new OpenIdServerSettings { UserinfoEndpointPath = "/existing/userinfo" };
        var server = new Mock<IOpenIdServerService>();
        server.Setup(service => service.LoadSettingsAsync()).ReturnsAsync(settings);
        server.Setup(service => service.ValidateSettingsAsync(settings)).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        var manager = new Mock<IOpenIdApplicationManager>();
        OpenIdApplicationDescriptor created = null;
        manager.Setup(value => value.CreateAsync(It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<OpenIddictApplicationDescriptor, CancellationToken>((value, _) => created = (OpenIdApplicationDescriptor)value);
        var service = new RemoteManagementCliConfigurationService(manager.Object, server.Object);

        await service.ConfigureAsync();

        Assert.Equal("orchardcore-cli", created.ClientId);
        Assert.Equal(OpenIddictConstants.ClientTypes.Public, created.ClientType);
        Assert.Contains(new Uri("http://127.0.0.1/callback"), created.RedirectUris);
        Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.DeviceCode, created.Permissions);
        Assert.True(settings.AllowDeviceAuthorizationFlow);
        Assert.Equal("/connect/device", settings.DeviceAuthorizationEndpointPath);
        Assert.Equal("/existing/userinfo", settings.UserinfoEndpointPath);
        manager.Verify(value => value.FindByClientIdAsync("orchardcore-mcp", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CliConfigure_ExistingApplication_PreservesRolesAndAdditionalCallbacks()
    {
        var settings = new OpenIdServerSettings();
        var server = new Mock<IOpenIdServerService>();
        server.Setup(service => service.LoadSettingsAsync()).ReturnsAsync(settings);
        server.Setup(service => service.ValidateSettingsAsync(settings)).ReturnsAsync(ImmutableArray<ValidationResult>.Empty);
        var manager = new Mock<IOpenIdApplicationManager>();
        var application = new object();
        manager.Setup(value => value.FindByClientIdAsync("orchardcore-cli", It.IsAny<CancellationToken>())).ReturnsAsync(application);
        manager.Setup(value => value.PopulateAsync(It.IsAny<OpenIdApplicationDescriptor>(), application, It.IsAny<CancellationToken>()))
            .Callback<OpenIddictApplicationDescriptor, object, CancellationToken>((value, _, _) =>
            {
                value.RedirectUris.Add(new Uri("https://existing.example/callback"));
                ((OpenIdApplicationDescriptor)value).Roles.Add("ExistingRole");
            });
        OpenIdApplicationDescriptor updated = null;
        manager.Setup(value => value.UpdateAsync(application, It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, value, _) => updated = (OpenIdApplicationDescriptor)value);

        await new RemoteManagementCliConfigurationService(manager.Object, server.Object).ConfigureAsync();

        Assert.Contains("ExistingRole", updated.Roles);
        Assert.Contains(new Uri("https://existing.example/callback"), updated.RedirectUris);
    }

    [Fact]
    public async Task McpConfigure_NewClient_RequiresConsentPkceAndManagementScope()
    {
        var manager = new Mock<IOpenIdApplicationManager>();
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>();
        OpenIdApplicationDescriptor created = null;
        manager.Setup(value => value.CreateAsync(It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<OpenIddictApplicationDescriptor, CancellationToken>((value, _) => created = (OpenIdApplicationDescriptor)value);
        var service = new RemoteManagementMcpConfigurationService(manager.Object, tenant.Object);

        await service.ConfigureAsync(new RemoteManagementMcpViewModel { RedirectUris = "https://client.example/callback" });

        Assert.Equal("orchardcore-mcp", created.ClientId);
        Assert.Equal(OpenIddictConstants.ClientTypes.Public, created.ClientType);
        Assert.Equal(OpenIddictConstants.ConsentTypes.Explicit, created.ConsentType);
        Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, created.Requirements);
        Assert.Contains(OpenIddictConstants.Permissions.Prefixes.Scope + RemoteManagementConstants.ManagementScope, created.Permissions);
        Assert.DoesNotContain(OpenIddictConstants.Permissions.GrantTypes.DeviceCode, created.Permissions);
        Assert.DoesNotContain(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, created.Permissions);
        Assert.Empty(created.Roles);
        Assert.Null(created.ClientSecret);
        tenant.Verify(value => value.ConfigureAsync(), Times.Once);
        manager.Verify(value => value.FindByClientIdAsync("orchardcore-cli", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("http://untrusted.example/callback")]
    [InlineData("https://client.example/callback#fragment")]
    [InlineData("https://user:password@client.example/callback")]
    [InlineData("https://client.example/*")]
    [InlineData("javascript:alert(1)")]
    [InlineData("")]
    public async Task McpConfigure_InvalidCallback_DoesNotModifySharedSettingsOrApplications(string callback)
    {
        var manager = new Mock<IOpenIdApplicationManager>(MockBehavior.Strict);
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>(MockBehavior.Strict);
        var service = new RemoteManagementMcpConfigurationService(manager.Object, tenant.Object);
        await Assert.ThrowsAsync<ValidationException>(() => service.ConfigureAsync(new RemoteManagementMcpViewModel { RedirectUris = callback }));
        manager.VerifyNoOtherCalls();
        tenant.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task McpConfigure_ExistingUnrelatedClient_DoesNotConvertOrOverwriteIt()
    {
        var application = new object();
        var manager = new Mock<IOpenIdApplicationManager>();
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>(MockBehavior.Strict);
        manager.Setup(value => value.FindByClientIdAsync("other-client", It.IsAny<CancellationToken>())).ReturnsAsync(application);
        var service = new RemoteManagementMcpConfigurationService(manager.Object, tenant.Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.ConfigureAsync(new RemoteManagementMcpViewModel
        {
            ClientId = "other-client", RedirectUris = "https://client.example/callback",
        }));

        manager.Verify(value => value.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()), Times.Never);
        tenant.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task McpConfigure_ExistingManagedClient_UpdatesExactCallbacksAndPreservesRoles()
    {
        var application = new object();
        var manager = new Mock<IOpenIdApplicationManager>();
        var tenant = new Mock<IRemoteManagementTenantConfigurationService>();
        manager.Setup(value => value.FindByClientIdAsync("orchardcore-mcp", It.IsAny<CancellationToken>())).ReturnsAsync(application);
        manager.Setup(value => value.PopulateAsync(It.IsAny<OpenIdApplicationDescriptor>(), application, It.IsAny<CancellationToken>()))
            .Callback<OpenIddictApplicationDescriptor, object, CancellationToken>((value, _, _) =>
            {
                value.ClientType = OpenIddictConstants.ClientTypes.Public;
                value.Properties["orchardcore:remote-management:client"] = JsonSerializer.SerializeToElement("mcp");
                value.RedirectUris.Add(new Uri("https://old.example/callback"));
                value.Permissions.Add("custom-permission");
                ((OpenIdApplicationDescriptor)value).Roles.Add("ExistingRole");
            });
        OpenIdApplicationDescriptor updated = null;
        manager.Setup(value => value.UpdateAsync(application, It.IsAny<OpenIdApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .Callback<object, OpenIddictApplicationDescriptor, CancellationToken>((_, value, _) => updated = (OpenIdApplicationDescriptor)value);

        await new RemoteManagementMcpConfigurationService(manager.Object, tenant.Object).ConfigureAsync(
            new RemoteManagementMcpViewModel { RedirectUris = "http://127.0.0.1:5123/callback" });

        Assert.Equal(new Uri("http://127.0.0.1:5123/callback"), Assert.Single(updated.RedirectUris));
        Assert.Equal(OpenIddictConstants.ApplicationTypes.Native, updated.ApplicationType);
        Assert.Contains("custom-permission", updated.Permissions);
        Assert.Contains("ExistingRole", updated.Roles);
    }
}
