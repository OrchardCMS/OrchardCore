using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ManagementRouteConventionTests
{
    private static readonly string[] _features =
    [
        "OrchardCore.BackgroundTasks",
        "OrchardCore.Deployment",
        "OrchardCore.Deployment.Remote",
        "OrchardCore.Email",
        "OrchardCore.Layers",
        "OrchardCore.Localization",
        "OrchardCore.Media",
        "OrchardCore.Media.Cache",
        "OrchardCore.Placements",
        "OrchardCore.Queries",
        "OrchardCore.Shortcodes.Templates",
        "OrchardCore.UrlRewriting",
        "OrchardCore.Workflows",
    ];

    [Fact]
    public async Task ActionRoutes_OfManagementOperations_UseTheColonForm()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = (await features.GetAvailableFeaturesAsync()).Where(feature => _features.Contains(feature.Id, StringComparer.Ordinal)).ToArray();
            await features.EnableFeaturesAsync(available, force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        // Loading a named endpoint initializes the endpoints of the enabled features.
        var warmup = await context.Client.GetAsync("__eptnametest?name=ApiListContentItems", TestContext.Current.CancellationToken);
        warmup.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(scope =>
        {
            var posts = scope.ServiceProvider.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.Metadata.GetMetadata<CliOperationMetadata>() is not null
                    && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true)
                .Select(endpoint => (Route: Normalize(endpoint.RoutePattern.RawText), endpoint.Metadata.GetMetadata<CliOperationMetadata>().Verb))
                .ToArray();

            // A route named after the verb of its command is an action on the resource before it.
            Assert.DoesNotContain(posts, post => post.Route.EndsWith('/' + post.Verb, StringComparison.Ordinal));

            string[] expected =
            [
                "api/background-tasks:validate",
                "api/background-tasks/by-name:enable",
                "api/background-tasks/by-name:disable",
                "api/content:create-draft",
                "api/content:validate",
                "api/content/{contentItemId}:draft",
                "api/content/{contentItemId}:publish",
                "api/content/{contentItemId}:unpublish",
                "api/content/{contentItemId}:validate",
                "api/content/versions/{contentItemVersionId}:restore",
                "api/deployment/operations:export",
                "api/deployment/operations:import",
                "api/deployment/targets/{id}:send",
                "api/email:test",
                "api/layers:validate",
                "api/localization/messages:resolve",
                "api/media/cache:purge",
                "api/placements:validate",
                "api/queries:validate",
                "api/queries/named/{name}:execute",
                "api/shortcode-templates:validate",
                "api/url-rewriting/rules:validate",
                "api/workflows/types:validate",
            ];
            Assert.Empty(expected.Except(posts.Select(post => post.Route), StringComparer.Ordinal));

            return Task.CompletedTask;
        });
    }

    private static string Normalize(string route) => route.TrimStart('/').Replace("//", "/", StringComparison.Ordinal);
}
