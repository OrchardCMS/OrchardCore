using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tenants.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

/// <summary>
/// Keeps the examples of the OpenAPI document valid, by posting them to their endpoints.
/// </summary>
public class OpenApiExampleTests
{
    private static readonly string[] _credentialProperties = ["password", "clientSecret"];

    [Fact]
    public async Task Examples_PostedToTheirEndpoints_AreAcceptedAndDescribeTheResponse()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        var cancellationToken = TestContext.Current.CancellationToken;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.EnableFeaturesAsync(available.Where(feature => feature.Id is "OrchardCore.Queries.Sql"), force: true);
        });
        var credentials = RemoteManagementClientCredentials.Generate();
        Assert.True(SiteContext.ShellHost.TryGetSettings(site.TenantName, out var settings));
        Assert.True(await RemoteManagementProvisioning.ConfigureAsync(SiteContext.ShellHost, settings, credentials));
        await AuthenticateAsync(site.Client, credentials, cancellationToken);

        using var openApi = await site.Client.GetAsync("openapi/v1.json", cancellationToken);
        var payload = await openApi.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(openApi.IsSuccessStatusCode, payload);
        var document = JsonNode.Parse(payload);

        foreach (var operationId in new[] { "ApiCreateRole", "ApiCreateUser", "ApiCreateQuery", "ApiCreateOpenIdApplication", "ApiCreateDraftContentItem", "ApiCreateContentItem" })
        {
            var (path, operation) = FindOperation(document, operationId);
            var request = operation["requestBody"]["content"]["application/json"]["example"];
            Assert.NotNull(request);
            AssertCredentialsArePlaceholders(request);

            using var content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await site.Client.PostAsync(path.TrimStart('/'), content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.True(response.IsSuccessStatusCode, $"{operationId} refused its example with {(int)response.StatusCode}: {body}");

            var example = operation["responses"][((int)response.StatusCode).ToString()]["content"]["application/json"]["example"];
            Assert.NotNull(example);
            AssertDescribes(operationId, example, JsonNode.Parse(body));
        }
    }

    [Fact]
    public async Task TenantExample_PostedToTheHandler_IsAcceptedAndDescribesTheResponse()
    {
        // The other examples go through HTTP. The tenant one needs the Default tenant, which has no
        // OpenID server on the test site, so it goes through the handler of the endpoint with its metadata.
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        app.AddTenantManagementEndpoints();
        var metadata = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .Single(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "ApiCreateTenantManagement")
            .Metadata.GetRequiredMetadata<OpenApiExampleMetadata>();

        var request = metadata.Request.Deserialize<TenantManagementEndpoints.TenantCreateRequest>(JsonSerializerOptions.Web);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var scope = await SiteContext.ShellHost.GetScopeAsync(ShellSettings.DefaultShellName);
        SiteContext.HttpContextAccessor.HttpContext = scope.ShellContext.CreateHttpContext();
        try
        {
            await scope.UsingAsync(async childScope =>
            {
                var services = childScope.ServiceProvider;
                var http = new DefaultHttpContext { RequestServices = services };
                http.Request.Scheme = "https";
                http.Request.Host = new HostString("cms.example.com");
                var result = await TenantManagementEndpoints.CreateAsync(http, request,
                    services.GetRequiredService<IShellHost>(), services.GetRequiredService<ShellSettings>(),
                    services.GetRequiredService<IShellSettingsManager>(), authorization.Object,
                    services.GetRequiredService<IDataProtectionProvider>(), services.GetRequiredService<IClock>(),
                    services.GetRequiredService<IEnumerable<DatabaseProvider>>(), services.GetRequiredService<ITenantValidator>(),
                    services.GetRequiredService<TenantDatabasePatternResolver>(), services.GetRequiredService<IStringLocalizer<TenantApiController>>(),
                    services.GetRequiredService<ILogger<TenantApiController>>(), services.GetRequiredService<IDistributedLock>());

                var created = result is Ok<TenantManagementEndpoints.TenantResponse> retried
                    ? retried.Value
                    : Assert.IsType<Created<TenantManagementEndpoints.TenantResponse>>(result).Value;
                AssertDescribes("ApiCreateTenantManagement", metadata.Response, JsonSerializer.SerializeToNode(created, JsonSerializerOptions.Web));
            });
        }
        finally
        {
            SiteContext.HttpContextAccessor.HttpContext = null;
        }

        if (SiteContext.ShellHost.TryGetSettings(request.Name, out var settings))
        {
            await SiteContext.ShellHost.RemoveShellSettingsAsync(settings);
        }
    }

    private static async Task AuthenticateAsync(HttpClient client, RemoteManagementClientCredentials credentials, CancellationToken cancellationToken)
    {
        using var tokenResponse = await client.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = credentials.ClientId,
            ["client_secret"] = credentials.ClientSecret,
            ["scope"] = RemoteManagementConstants.ManagementScope,
        }), cancellationToken);
        var payload = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(tokenResponse.IsSuccessStatusCode, payload);
        using var token = JsonDocument.Parse(payload);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.RootElement.GetProperty("access_token").GetString());
    }

    private static (string Path, JsonNode Operation) FindOperation(JsonNode document, string operationId)
    {
        foreach (var (path, item) in document["paths"].AsObject())
        {
            foreach (var (_, operation) in item.AsObject())
            {
                if (operation is JsonObject && operation["operationId"]?.GetValue<string>() == operationId)
                {
                    return (path, operation);
                }
            }
        }

        Assert.Fail($"The OpenAPI document has no {operationId} operation.");
        return default;
    }

    /// <summary>
    /// A credential in an example has to read as a placeholder, as people copy examples.
    /// </summary>
    private static void AssertCredentialsArePlaceholders(JsonNode example)
    {
        foreach (var property in _credentialProperties)
        {
            if (example[property] is { } value)
            {
                Assert.Contains("replace", value.GetValue<string>(), StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>
    /// Every property of the example exists in the actual response, with the same JSON kind unless one of them is null.
    /// </summary>
    private static void AssertDescribes(string path, JsonNode example, JsonNode actual)
    {
        if (example is null || actual is null)
        {
            return;
        }

        Assert.True(Kind(example) == Kind(actual), $"{path}: the example has a {example.GetValueKind()} and the response a {actual.GetValueKind()}.");

        if (example is JsonObject exampleObject)
        {
            var actualObject = actual.AsObject();
            foreach (var (name, value) in exampleObject)
            {
                Assert.True(actualObject.ContainsKey(name), $"{path}.{name} is in the example, not in the response: {actual.ToJsonString()}");
                AssertDescribes(path + "." + name, value, actualObject[name]);
            }
        }
        else if (example is JsonArray { Count: > 0 } exampleArray && actual.AsArray().Count > 0)
        {
            AssertDescribes(path + "[]", exampleArray[0], actual.AsArray()[0]);
        }
    }

    private static JsonValueKind Kind(JsonNode node) => node.GetValueKind() is JsonValueKind.False ? JsonValueKind.True : node.GetValueKind();
}
