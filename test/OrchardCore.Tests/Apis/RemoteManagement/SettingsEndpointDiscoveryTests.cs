using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SettingsEndpointDiscoveryTests
{
    [Fact]
    public async Task EnabledFeatures_ExposeSettingsEndpointsAndCapabilities()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.CustomSettings");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        var warmup = await context.Client.GetAsync(
            "__eptnametest?name=ApiGetSiteSettings",
            TestContext.Current.CancellationToken);
        warmup.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var endpoints = scope.ServiceProvider
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .ToArray();

            AssertOperation(endpoints, "/api/settings", "GET", "ApiGetSiteSettings", ["settings"], "show");
            AssertOperation(endpoints, "/api/settings", "PUT", "ApiUpdateSiteSettings", ["settings"], "update", CliInputMode.Json);
            AssertOperation(endpoints, "/api/settings/schema", "GET", "ApiGetSiteSettingsSchema", ["settings"], "schema");
            AssertOperation(endpoints, "/api/custom-settings", "GET", "ApiListCustomSettings", ["custom-settings"], "list");
            AssertOperation(endpoints, "/api/custom-settings/{name}", "GET", "ApiGetCustomSettings", ["custom-settings"], "show");
            AssertOperation(endpoints, "/api/custom-settings/{name}", "PUT", "ApiUpdateCustomSettings", ["custom-settings"], "update", CliInputMode.Json);
            AssertOperation(endpoints, "/api/custom-settings/{name}/schema", "GET", "ApiGetCustomSettingsSchema", ["custom-settings"], "schema");

            var capabilities = new List<RemoteManagementCapability>();
            foreach (var provider in scope.ServiceProvider.GetServices<IRemoteManagementCapabilityProvider>())
            {
                capabilities.AddRange(await provider.GetCapabilitiesAsync());
            }

            Assert.Contains(capabilities, capability => capability.Id == "settings");
            Assert.Contains(capabilities, capability => capability.Id == "custom-settings");
        });
    }

    private static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await shellFeaturesManager.GetAvailableFeaturesAsync())
                .Where(feature => featureIds.Contains(feature.Id, StringComparer.Ordinal))
                .ToArray();

            await shellFeaturesManager.EnableFeaturesAsync(features, force: true);
        });
    }

    private static void AssertOperation(
        IEnumerable<RouteEndpoint> endpoints,
        string route,
        string method,
        string endpointName,
        string[] commandGroup,
        string verb,
        CliInputMode inputMode = CliInputMode.Options)
    {
        var endpoint = endpoints.Single(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText?.Trim('/'), route.Trim('/'), StringComparison.Ordinal)
            && endpoint.Metadata.GetRequiredMetadata<IHttpMethodMetadata>().HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase));

        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));

        var cli = endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>();
        Assert.Equal(commandGroup, cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);
        Assert.Equal(inputMode, cli.InputMode);
    }
}
