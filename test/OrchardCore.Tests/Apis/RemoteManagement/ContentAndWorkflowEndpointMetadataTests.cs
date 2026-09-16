using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentTypes.Management;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ContentAndWorkflowEndpointMetadataTests
{
    [Fact]
    public async Task Endpoints_ExposeContentAndWorkflowManagementMetadata()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await DisableFeaturesAsync(context, "OrchardCore.Features");
        await EnableFeaturesAsync(context, "OrchardCore.ContentTypes", "OrchardCore.Autoroute", "OrchardCore.Flows", "OrchardCore.Workflows");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        var warmup = await context.Client.GetAsync("__eptnametest?name=ApiListContentItems", TestContext.Current.CancellationToken);
        warmup.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var endpoints = scope.ServiceProvider.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToArray();

            AssertOperation(endpoints, "/api/content-definition/types", "GET", "ApiListContentTypeDefinitions", ["content", "types"], "list");
            AssertOperation(endpoints, "/api/content-definition/field-types", "GET", "ApiListContentFieldTypes", ["content", "field-types"], "list");
            AssertOperation(endpoints, "/api/content-definition/settings", "GET", "ApiListContentDefinitionSettingsSchemas", ["content", "settings"], "list");
            AssertOperation(endpoints, "/api/content-definition/settings/{name}", "GET", "ApiGetContentDefinitionSettingsSchema", ["content", "settings"], "show");
            AssertOperation(endpoints, "/api/content", "GET", "ApiListContentItems", ["content", "items"], "list");
            AssertOperation(endpoints, "/api/content/{contentItemId}/publish", "POST", "ApiPublishContentItem", ["content", "items"], "publish");
            AssertOperation(endpoints, "/api/content/schema/{contentType}", "GET", "ApiGetContentItemSchema", ["content", "items"], "schema");
            AssertOperation(endpoints, "/api/content/{contentItemId}/versions", "GET", "ApiListContentItemVersions", ["content", "versions"], "list");
            AssertOperation(endpoints, "/api/content/versions/{contentItemVersionId}", "GET", "ApiGetContentItemVersion", ["content", "versions"], "show");
            AssertOperation(endpoints, "/api/content/versions/{contentItemVersionId}/render", "GET", "ApiRenderContentItemVersion", ["content", "versions"], "render");
            AssertOperation(endpoints, "/api/content/versions/{contentItemVersionId}/restore", "POST", "ApiRestoreContentItemVersion", ["content", "versions"], "restore");
            AssertOperation(endpoints, "/api/content/versions/{contentItemVersionId}", "DELETE", "ApiDeleteContentItemVersion", ["content", "versions"], "delete");
            AssertOperation(endpoints, "/api/workflows/types", "POST", "ApiCreateWorkflowType", ["workflow", "types"], "create", CliInputMode.Json);
            AssertOperation(endpoints, "/api/workflows/activity-types", "GET", "ApiListWorkflowActivityTypes", ["workflow", "activity-types"], "list");
            AssertOperation(endpoints, "/api/workflows/types/{workflowTypeId}/execute", "POST", "ApiExecuteWorkflowType", ["workflow", "types"], "execute", CliInputMode.Json);
            AssertOperation(endpoints, "/api/workflows/instances/{workflowId}", "DELETE", "ApiCancelWorkflowInstance", ["workflow", "instances"], "cancel");
            Assert.DoesNotContain(
                endpoints
                    .Select(endpoint => endpoint.Metadata.GetMetadata<CliOperationMetadata>())
                    .Where(metadata => metadata is not null)
                    .SelectMany(metadata => metadata.Aliases),
                alias => string.Equals(alias, "ls", StringComparison.Ordinal));

            var capabilities = new List<RemoteManagementCapability>();
            foreach (var provider in scope.ServiceProvider.GetServices<IRemoteManagementCapabilityProvider>())
            {
                capabilities.AddRange(await provider.GetCapabilitiesAsync());
            }

            Assert.Contains(capabilities, capability => capability.Id == "content-definitions");
            Assert.Contains(capabilities, capability => capability.Id == "content-items");
            Assert.Contains(capabilities, capability => capability.Id == "workflows");

            var settingsSchemas = scope.ServiceProvider
                .GetServices<IContentDefinitionManagementSchemaProvider>()
                .SelectMany(provider => provider.GetSchemas());
            var autorouteSettings = Assert.Single(settingsSchemas, schema => schema.Name == "AutoroutePartSettings");
            Assert.Equal(ContentDefinitionManagementSchemaScope.ContentTypePart, autorouteSettings.Scope);
            Assert.Equal("AutoroutePart", autorouteSettings.AppliesTo);
            Assert.Contains(settingsSchemas, schema =>
                schema.Name == "FlowPartSettings" &&
                schema.AppliesTo == "FlowPart");
            Assert.Contains(settingsSchemas, schema =>
                schema.Name == "BagPartSettings" &&
                schema.AppliesTo == "BagPart");

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

    private static async Task DisableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(feature => featureIds.Contains(feature.Id, StringComparer.Ordinal))
                .ToArray();

            await shellFeaturesManager.DisableFeaturesAsync(features, force: true);
        });
    }

    private static void AssertOperation(
        IEnumerable<RouteEndpoint> endpoints,
        string route,
        string method,
        string endpointName,
        string[] commandGroup,
        string verb,
        CliInputMode expectedInputMode = CliInputMode.Options)
    {
        var endpoint = endpoints.FirstOrDefault(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText?.Trim('/'), route.Trim('/'), StringComparison.Ordinal)
            && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase) == true);

        Assert.NotNull(endpoint);
        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));

        var cli = endpoint.Metadata.GetMetadata<CliOperationMetadata>();
        Assert.NotNull(cli);
        Assert.Equal(commandGroup, cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);
        Assert.Equal(expectedInputMode, cli.InputMode);

        var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        Assert.Contains(authorizeData, metadata => string.Equals(metadata.AuthenticationSchemes, OrchardCoreConstants.AuthenticationSchemes.Api, StringComparison.Ordinal));
    }
}
