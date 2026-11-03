using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ManagementDeleteContractTests
{
    private static readonly string[] _features =
    [
        "OrchardCore.ContentTypes",
        "OrchardCore.Localization",
        "OrchardCore.Workflows",
    ];

    // The other routes require a bearer token, which the test host can't issue. They are covered by
    // the tests of their handlers, and with a real token by '.scripts/remote-management/delete-contract-smoke.py'.
    private static readonly string[] _routes =
    [
        "api/content/missing",
        "api/content/versions/missing",
        "api/content-definition/parts/Missing",
        "api/content-definition/parts/Missing/fields/Missing",
        "api/content-definition/types/Missing",
        "api/localization/cultures/de",
        "api/localization/cultures/not-a-culture",
        "api/workflows/instances/missing",
        "api/workflows/types/missing",
    ];

    [Fact]
    public async Task Delete_MissingResource_ReturnsNotFoundProblem()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.EnableFeaturesAsync(available.Where(feature => _features.Contains(feature.Id, StringComparer.Ordinal)).ToArray(), force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        var failures = new List<string>();
        foreach (var route in _routes)
        {
            using var response = await context.Client.DeleteAsync(route, TestContext.Current.CancellationToken);

            // A route that doesn't exist also returns 404, without a problem.
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound
                || response.Content.Headers.ContentType?.MediaType != "application/problem+json")
            {
                failures.Add($"{route}: {(int)response.StatusCode} {response.Content.Headers.ContentType?.MediaType}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(", ", failures));
    }
}
