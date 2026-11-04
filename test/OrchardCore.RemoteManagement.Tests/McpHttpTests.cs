using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Handlers;
using OrchardCore.RemoteManagement;
using OrchardCore.RemoteManagement.Mcp;
using OrchardCore.Security;

using Xunit;

namespace OrchardCore.RemoteManagement.Tests;

public class McpHttpTests
{
    [Fact]
    public async Task Mcp_AnonymousRequest_ReturnsBearerChallengeWithTenantMetadata()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        using var response = await SendAsync(client, "tools/list");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Equal("Bearer", challenge.Scheme);
        Assert.Contains("/tenant-a/.well-known/oauth-protected-resource/mcp", challenge.Parameter);

        using var metadata = await client.GetAsync("/tenant-a/.well-known/oauth-protected-resource/mcp", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, metadata.StatusCode);
        using var document = JsonDocument.Parse(await metadata.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("http://localhost/tenant-a/mcp", document.RootElement.GetProperty("resource").GetString());
        Assert.Equal("http://localhost/tenant-a/", document.RootElement.GetProperty("authorization_servers")[0].GetString());
    }

    [Fact]
    public async Task Mcp_InvalidToken_KeepsErrorInSingleChallenge()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");
        using var response = await SendAsync(client, "tools/list");
        var challenge = Assert.Single(response.Headers.WwwAuthenticate);
        Assert.Contains("resource_metadata=", challenge.Parameter);
        Assert.Contains("error=\"invalid_token\"", challenge.Parameter);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    public async Task Mcp_StatelessStreamRequest_ReturnsMethodNotAllowed(string method)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");
        using var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/tenant-a/mcp"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("denied", HttpStatusCode.Forbidden)]
    [InlineData("invalid", HttpStatusCode.Unauthorized)]
    public async Task Mcp_WithoutManagementPermission_RejectsTransport(string token, HttpStatusCode expected)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await SendAsync(client, "tools/list");
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Mcp_AuthenticatedRequest_ListsAndInvokesOriginalEndpoint()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");
        using var list = await SendAsync(client, "tools/list");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listBody = await list.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("widgets_show", listBody);

        using var call = await SendAsync(client, "tools/call", """{"name":"widgets_show","arguments":{"path":{"id":"42"}}}""");
        Assert.Equal(HttpStatusCode.OK, call.StatusCode);
        var callBody = await call.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("tenant-a", callBody);
        Assert.Contains("42", callBody);
        Assert.DoesNotContain("\"isError\":true", callBody);
    }

    [Fact]
    public async Task Mcp_OperationPermissionDenied_ReturnsToolError()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "management-only");
        using var call = await SendAsync(client, "tools/call", """{"name":"widgets_show","arguments":{"path":{"id":"42"}}}""");
        Assert.Equal(HttpStatusCode.OK, call.StatusCode);
        var body = await call.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"isError\":true", body);
        Assert.Contains("403", body);
    }

    [Fact]
    public async Task Mcp_CrossOriginRequest_IsRejected()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");
        client.DefaultRequestHeaders.Add("Origin", "https://untrusted.example");
        using var response = await SendAsync(client, "tools/list");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Mcp_CookieWithoutBearer_IsRejected()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("Cookie", "admin=allowed");
        using var response = await SendAsync(client, "tools/list");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("widgets_unknown", "{}", "Unknown or unavailable")]
    [InlineData("widgets_show", "{\"path\":{\"id\":\"filtered\"}}", "409")]
    public async Task Mcp_UnknownToolOrEndpointFilter_ReturnsToolError(string tool, string arguments, string expected)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");
        using var response = await SendAsync(client, "tools/call", $$"""{"name":"{{tool}}","arguments":{{arguments}}}""");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"isError\":true", body);
        Assert.Contains(expected, body);
    }

    [Fact]
    public async Task Mcp_JsonBody_UsesOriginalEndpointBinding()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");
        using var response = await SendAsync(client, "tools/call", """{"name":"widgets_update","arguments":{"path":{"id":"42"},"body":{"title":"Example"}}}""");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Example", body);
        Assert.DoesNotContain("\"isError\":true", body);
    }

    [Fact]
    public async Task Mcp_ToolCall_ExposesTheDispatchedRequestAndRestoresTheOuterOne()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");

        using var response = await SendAsync(client, "tools/call", """{"name":"widgets_ambient","arguments":{}}""");

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("same", body);
        Assert.Contains("true", body);
        Assert.Equal("true", response.Headers.GetValues("X-Outer-Request-Kept").Single());
    }

    [Fact]
    public async Task Mcp_Initialize_ReportsTheOrchardCoreVersion()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");

        using var response = await SendAsync(client, "initialize", """{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1"}}""");

        Assert.Contains($$"""
            "version":"{{OrchardCore.Modules.Manifest.ManifestConstants.OrchardCoreVersion}}"
            """, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("dynamic-resource", HttpStatusCode.OK)]
    [InlineData("dynamic-other-resource", HttpStatusCode.Forbidden)]
    [InlineData("dynamic-no-resource", HttpStatusCode.Forbidden)]
    [InlineData("client-credentials", HttpStatusCode.OK)]
    [InlineData("no-client", HttpStatusCode.OK)]
    public async Task Mcp_DynamicClientToken_RequiresTheMcpAudience(string token, HttpStatusCode expected)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await SendAsync(client, "tools/list");

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("dynamic-resource", HttpStatusCode.Forbidden)]
    [InlineData("dynamic-no-resource", HttpStatusCode.Forbidden)]
    [InlineData("client-credentials", HttpStatusCode.OK)]
    [InlineData("no-client", HttpStatusCode.OK)]
    public async Task Api_DynamicClientToken_IsRefused(string token, HttpStatusCode expected)
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/tenant-a/api/managed/1", TestContext.Current.CancellationToken);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Mcp_DynamicClientToken_CanInvokeTheToolOfARefusedOperation()
    {
        await using var app = await CreateApplicationAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dynamic-resource");

        using var response = await SendAsync(client, "tools/call", """{"name":"managed_show","arguments":{"path":{"id":"42"}}}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("statusCode", body);
        Assert.Contains("200", body);
        Assert.DoesNotContain("403", body);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("true", true)]
    public async Task Mcp_ConfirmationRequiredOperation_IsAToolOnlyWhenConfigured(string allowed, bool listed)
    {
        await using var app = await CreateApplicationAsync(allowed is null ? [] : new() { ["RemoteManagement:Mcp:AllowConfirmationRequiredTools"] = allowed });
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "allowed");

        using var list = await SendAsync(client, "tools/list");
        Assert.Equal(listed, (await list.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Contains("widgets_delete"));

        using var call = await SendAsync(client, "tools/call", """{"name":"widgets_delete","arguments":{"path":{"id":"42"}}}""");
        var body = await call.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(listed, !body.Contains("Unknown or unavailable"));
    }

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, string method, string parameters = "{}")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/tenant-a/mcp")
        {
            Content = new StringContent($$"""{"jsonrpc":"2.0","id":1,"method":"{{method}}","params":{{parameters}}}""", Encoding.UTF8, "application/json"),
        };
        request.Headers.Accept.ParseAdd("application/json, text/event-stream");
        request.Headers.Add("MCP-Protocol-Version", "2025-11-25");
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<WebApplication> CreateApplicationAsync(Dictionary<string, string> configuration = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IShellConfiguration>(new ShellConfiguration(new ConfigurationBuilder().AddInMemoryCollection(configuration ?? []).Build()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, TestBearerHandler>(OrchardCoreConstants.AuthenticationSchemes.Api, _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IAuthorizationHandler, ManagementPermissionHandler>();
        var applications = new Mock<IOpenIdApplicationManager>();
        var dynamicClient = new object();
        applications.Setup(manager => manager.FindByClientIdAsync("dynamic-client", It.IsAny<CancellationToken>())).ReturnsAsync(dynamicClient);
        applications.Setup(manager => manager.GetPropertiesAsync(dynamicClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty.Add("orchardcore:remote-management:dynamic-mcp", JsonSerializer.SerializeToElement(true)));
        builder.Services.AddSingleton(applications.Object);
        builder.Services.AddScoped<IAuthorizationHandler, McpResourceAudienceAuthorizationHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, McpClientApiAccessAuthorizationHandler>();
        builder.Services.AddScoped(_ => new RemoteManagementManifestService([], Options.Create(new OpenIddictServerOptions()), new ShellSettings { Name = "TenantA" }));
        var startup = new McpStartup();
        startup.ConfigureServices(builder.Services);
        var app = builder.Build();
        app.Use((context, next) =>
        {
            // Reads the accessor from the flow of the outer request, once the tool call completes.
            context.Response.OnStarting(() =>
            {
                var accessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();
                context.Response.Headers["X-Outer-Request-Kept"] = ReferenceEquals(accessor.HttpContext, context) ? "true" : "false";
                return Task.CompletedTask;
            });

            return next(context);
        });
        app.UsePathBase("/tenant-a");
        app.UseRouting();
        startup.Configure(app, app, app.Services);
        app.UseAuthorization();
        app.MapGet("api/widgets/{id}", (string id, HttpContext context) => Results.Json(new { id, tenant = context.Request.PathBase.Value }))
            .WithName("ApiGetWidget")
            .WithCliCommand(new CliOperationMetadata(["widgets"], "show"))
            .AddEndpointFilter(async (context, next) => context.GetArgument<string>(0) == "filtered"
                ? Results.StatusCode(409) : await next(context))
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireClaim("widgets"));
        app.MapPut("api/widgets/{id}", (string id, WidgetInput input) => Results.Json(new { id, input.Title }))
            .WithName("ApiUpdateWidget")
            .WithCliCommand(new CliOperationMetadata(["widgets"], "update"))
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireClaim("widgets"));
        app.MapGet("api/widgets/ambient", (HttpContext context, IHttpContextAccessor accessor) => Results.Json(new { same = ReferenceEquals(accessor.HttpContext, context) }))
            .WithName("ApiGetAmbientWidget")
            .WithCliCommand(new CliOperationMetadata(["widgets"], "ambient"))
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireClaim("widgets"));
        app.MapGet("api/managed/{id}", (string id) => Results.Json(new { id }))
            .WithName("ApiGetManaged")
            .WithCliCommand(new CliOperationMetadata(["managed"], "show"))
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)));
        app.MapDelete("api/widgets/{id}", (string id) => Results.Json(new { id }))
            .WithName("ApiDeleteWidget")
            .WithCliCommand(new CliOperationMetadata(["widgets"], "delete") { RequiresConfirmation = true })
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireClaim("widgets"));
        await app.StartAsync();
        return app;
    }

    private sealed class WidgetInput
    {
        public string Title { get; set; }
    }

    private sealed class ManagementPermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.HasClaim("management", "true"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class TestBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers.WWWAuthenticate = Request.Headers.Authorization.Count > 0 ? "Bearer error=\"invalid_token\"" : "Bearer";
            return Task.CompletedTask;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers.Authorization.ToString();
            if (token is not ("Bearer allowed" or "Bearer management-only" or "Bearer denied" or
                "Bearer dynamic-resource" or "Bearer dynamic-other-resource" or "Bearer dynamic-no-resource" or
                "Bearer client-credentials" or "Bearer no-client"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim> { new("sub", "alice") };
            if (token != "Bearer denied")
            {
                claims.Add(new("management", "true"));
            }

            if (token == "Bearer allowed")
            {
                claims.Add(new("widgets", "true"));
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            switch (token)
            {
                case "Bearer dynamic-resource":
                    principal.SetPresenters("dynamic-client").SetAudiences("orchardcore", "http://localhost/tenant-a/mcp");
                    break;
                case "Bearer dynamic-other-resource":
                    principal.SetPresenters("dynamic-client").SetAudiences("orchardcore", "http://localhost/tenant-b/mcp");
                    break;
                case "Bearer dynamic-no-resource":
                    principal.SetPresenters("dynamic-client").SetAudiences("orchardcore");
                    break;
                case "Bearer client-credentials":
                    principal.SetPresenters("pre-registered-client").SetAudiences("orchardcore");
                    break;
            }

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
