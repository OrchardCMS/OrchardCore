using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Routing;

public class DuplicateEndpointNameTests
{
    // Regression test for the duplicate endpoint name crash.
    //
    // A recipe such as 'Blog' maps a dynamic controller route (Autoroute, HomeRoute, Sitemaps).
    // Mapping a dynamic controller route makes the shared controller endpoint data source also emit,
    // for every controller action, a second non-routable placeholder endpoint that carries a copy of
    // the action metadata, including its '[EndpointName]'. Attribute-routed API controllers (e.g.
    // OrchardCore.Queries) are decorated with '[EndpointName]', so their routable endpoint and their
    // placeholder endpoint ended up sharing the same endpoint name. Any link generation by name
    // (LinkGenerator.GetPathByName / GetUriByName) then threw an 'InvalidOperationException' reporting
    // duplicate endpoint names.
    [Fact]
    public async Task GetPathByName_ForAttributeRoutedApiController_UsesSingleNamedRouteEndpoint()
    {
        // Arrange.
        using var context = new SiteContext();

        await context.InitializeAsync();

        // Enable a feature that exposes an attribute-routed API controller decorated with
        // '[EndpointName]'.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            var queriesFeatures = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id == "OrchardCore.Queries")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], queriesFeatures, force: true);
        });

        // Act.
        // The '/__eptnametest' probe (registered by SiteStartup in the tenant pipeline) resolves the
        // 'LinkGenerator' and generates a link to the named endpoint. Before the fix this threw an
        // 'InvalidOperationException' about duplicate endpoint names, surfacing as a 500 response.
        var response = await context.Client.GetAsync(
            "__eptnametest?name=ApiExecuteQueryGet",
            TestContext.Current.CancellationToken);

        // Assert.
        response.EnsureSuccessStatusCode();

        var path = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("api/queries/test", path);
    }
}
