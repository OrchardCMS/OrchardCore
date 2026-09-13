using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.RemoteManagement;

public class RemoteManagementManifestServiceTests
{
    [Fact]
    public void CreateBootstrap_PathBasedTenant_UsesExactTenantUrlAndConfiguredGrants()
    {
        var options = new OpenIddictServerOptions();
        options.GrantTypes.Add("authorization_code");
        options.GrantTypes.Add("urn:ietf:params:oauth:grant-type:device_code");
        var service = CreateService(options);
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com");
        context.Request.PathBase = "/tenant-a";

        var manifest = service.CreateBootstrap(context.Request);

        Assert.Equal(new Uri("https://cms.example.com/tenant-a/"), manifest.Authentication.Authority);
        Assert.Equal(new Uri("https://cms.example.com/tenant-a/api/management/manifest"), manifest.ManagementManifestUrl);
        Assert.Contains("authorization_code", manifest.Authentication.GrantTypes);
        Assert.Contains("urn:ietf:params:oauth:grant-type:device_code", manifest.Authentication.GrantTypes);
        Assert.DoesNotContain("client_credentials", manifest.Authentication.GrantTypes);
        Assert.Contains(RemoteManagementConstants.ManagementScope, manifest.Authentication.Scopes);
    }

    [Fact]
    public async Task CreateAuthenticatedAsync_ProvidersConfigured_ReturnsTenantCapabilitiesAndCoordinates()
    {
        var capability = new RemoteManagementCapability
        {
            Id = "example.widgets",
            Version = "2.0",
        };
        var service = CreateService(
            new OpenIddictServerOptions(),
            new TestCapabilityProvider(capability));
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com");
        context.Request.PathBase = "/tenant-a";

        var manifest = await service.CreateAuthenticatedAsync(context.Request);

        Assert.Equal("TenantA", manifest.TenantId);
        Assert.Equal(new Uri("https://cms.example.com/tenant-a/openapi/v1.json"), manifest.OpenApiUrl);
        Assert.Equal(new Uri("https://json-schema.org/draft/2020-12/schema"), manifest.JsonSchemaDialect);
        Assert.Collection(
            manifest.Capabilities,
            value =>
            {
                Assert.Equal("example.widgets", value.Id);
                Assert.Equal("2.0", value.Version);
            });
    }

    [Fact]
    public void CreateBootstrap_CustomIssuer_UsesConfiguredAuthority()
    {
        var options = new OpenIddictServerOptions
        {
            Issuer = new Uri("https://identity.example.com/orchard"),
        };
        var service = CreateService(options);
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.internal");

        var manifest = service.CreateBootstrap(context.Request);

        Assert.Equal(options.Issuer, manifest.Authentication.Authority);
    }

    private static RemoteManagementManifestService CreateService(
        OpenIddictServerOptions options,
        params IRemoteManagementCapabilityProvider[] providers) =>
        new(
            providers,
            Options.Create(options),
            new ShellSettings { Name = "TenantA" });

    private sealed class TestCapabilityProvider : IRemoteManagementCapabilityProvider
    {
        private readonly RemoteManagementCapability _capability;

        public TestCapabilityProvider(RemoteManagementCapability capability)
        {
            _capability = capability;
        }

        public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
            ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([_capability]);
    }
}
