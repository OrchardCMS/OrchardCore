using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OpenIdDiscoveryFeatureTests
{
    [Fact]
    public async Task ManagementFeature_ControlsEndpointsAndCapabilityIndependentlyOfServer()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        foreach (var enabled in new[] { true, false, true })
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var feature = (await manager.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.OpenId.Management");
                if (enabled)
                {
                    await manager.EnableFeaturesAsync([feature], force: true);
                }
                else
                {
                    await manager.DisableFeaturesAsync([feature], force: true);
                }
            });
            await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
            var warmup = await context.Client.GetAsync("__eptnametest?name=ApiListContentItems", TestContext.Current.CancellationToken);
            warmup.EnsureSuccessStatusCode();
            await context.UsingTenantScopeAsync(async scope =>
            {
                var endpoints = scope.ServiceProvider.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
                    .Where(endpoint => endpoint.Metadata.GetMetadata<CliOperationMetadata>()?.Capability == "openid-management").ToArray();
                Assert.Equal(enabled ? 12 : 0, endpoints.Length);
                foreach (var endpoint in endpoints)
                {
                    var cli = endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>();
                    Assert.Equal("openid", cli.CommandGroup[0]);
                    Assert.Equal(cli.Verb == "rotate", cli.SecretResponse);
                    if (cli.Verb is "rotate" or "revoke")
                    {
                        Assert.Equal(["openid", "applications", "credentials"], cli.CommandGroup);
                        Assert.True(cli.RequiresConfirmation);
                    }
                    Assert.Equal(cli.CommandGroup[1] == "applications" && cli.Verb is "create" or "update" ? ["clientSecret"] : [], cli.SecretProperties);
                    var method = cli.Verb switch
                    {
                        "create" or "rotate" or "revoke" => "POST", "update" => "PUT", "delete" => "DELETE", _ => "GET",
                    };
                    Assert.Equal(method, Assert.Single(endpoint.Metadata.GetRequiredMetadata<IHttpMethodMetadata>().HttpMethods));
                    Assert.Contains(cli.Verb, new[] { "list", "show", "create", "update", "delete", "rotate", "revoke" });
                }
                var capabilities = new List<RemoteManagementCapability>();
                foreach (var provider in scope.ServiceProvider.GetServices<IRemoteManagementCapabilityProvider>())
                {
                    capabilities.AddRange(await provider.GetCapabilitiesAsync());
                }
                Assert.Equal(enabled ? 1 : 0, capabilities.Count(capability => capability.Id == "openid-management"));
                var features = await scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>().GetEnabledFeaturesAsync();
                Assert.DoesNotContain(features, feature => feature.Id == "OrchardCore.OpenId.Server");
            });
        }
    }
}
