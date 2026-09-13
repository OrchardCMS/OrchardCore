using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using Moq;
using OrchardCore.RemoteManagement;
using OrchardCore.RemoteManagement.Mcp;

using Xunit;

namespace OrchardCore.RemoteManagement.Tests;

public class McpToolTests
{
    [Fact]
    public void CreateTools_AnnotatedEndpoint_ProjectsParametersAndRecursiveBodySchema()
    {
        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], Document()));
        Assert.Equal("widgets_update", tool.Tool.Name);
        var schema = tool.Tool.InputSchema;
        Assert.Equal("#/$defs/Widget", schema.GetProperty("properties").GetProperty("body").GetProperty("$ref").GetString());
        var definitions = schema.GetProperty("$defs");
        Assert.False(definitions.TryGetProperty("Unused", out _));
        Assert.Equal("#/$defs/Widget", definitions.GetProperty("Widget").GetProperty("properties").GetProperty("child").GetProperty("$ref").GetString());
        Assert.Contains(schema.GetProperty("required").EnumerateArray(), value => value.GetString() == "path");
        Assert.Contains(schema.GetProperty("required").EnumerateArray(), value => value.GetString() == "body");
        Assert.True(tool.Tool.Annotations.DestructiveHint);
        Assert.False(tool.Tool.Annotations.ReadOnlyHint);
    }

    [Fact]
    public void CreateTools_HiddenStreamAndUnannotatedEndpoints_AreExcluded()
    {
        Assert.Empty(McpToolCatalog.CreateTools([
            CreateEndpoint(new CliOperationMetadata(["widgets"], "hidden") { Hidden = true }),
            CreateEndpoint(new CliOperationMetadata(["widgets"], "upload") { InputMode = CliInputMode.Stream }),
            new RouteEndpoint(_ => Task.CompletedTask, RoutePatternFactory.Parse("api/private"), 0, EndpointMetadataCollection.Empty, "private"),
        ], Document()));
    }

    [Fact]
    public void CreateTools_DuplicateNames_AreExcluded()
    {
        Assert.Empty(McpToolCatalog.CreateTools([CreateEndpoint(), CreateEndpoint()], Document()));
    }

    [Fact]
    public void CreateTools_DefaultBody_DoesNotRequireExplicitBody()
    {
        var metadata = new CliOperationMetadata(["widgets"], "update") { DefaultJsonBody = "{}" };
        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint(metadata)], Document()));
        Assert.DoesNotContain(tool.Tool.InputSchema.GetProperty("required").EnumerateArray(), value => value.GetString() == "body");
    }

    [Fact]
    public void CreateTools_TenantEndpointCollections_DoNotLeakOtherTenantTools()
    {
        Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], Document()));
        Assert.Empty(McpToolCatalog.CreateTools([], Document()));
    }

    [Fact]
    public async Task InvokeEndpointAsync_ValidArguments_PreservesTenantUserAndBindsInput()
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        var accessor = new HttpContextAccessor { HttpContext = parent };
        HttpContext dispatched = null;
        HttpContext ambient = null;
        string inputBody = null;
        var endpoint = CreateEndpoint(handler: async context =>
        {
            dispatched = context;
            ambient = accessor.HttpContext;
            inputBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            await context.Response.WriteAsync("{\"saved\":true}");
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, parent, """{"path":{"id":"a/b?c"},"query":{"search":"one&two"},"body":{"title":"Example"}}""");

        Assert.NotNull(dispatched);
        Assert.Same(services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(), dispatched.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        Assert.Same(parent.User, dispatched.User);
        Assert.Same(parent, ambient);
        Assert.Equal("/tenant-a", dispatched.Request.PathBase.Value);
        Assert.Equal("a/b?c", dispatched.Request.RouteValues["id"]);
        Assert.Equal("one&two", dispatched.Request.Query["search"]);
        Assert.Equal("{\"title\":\"Example\"}", inputBody);
        Assert.False(result.IsError == true);
        Assert.Same(parent, accessor.HttpContext);
        Assert.Contains("saved", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"path\":{},\"body\":{}}")]
    [InlineData("{\"path\":{\"id\":\"a\"},\"body\":{},\"url\":\"https://evil.example\"}")]
    [InlineData("{\"path\":{\"id\":\"a\"},\"body\":{},\"query\":{\"unexpected\":true}}")]
    public async Task InvokeEndpointAsync_InvalidArguments_DoesNotExecute(string arguments)
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        var invoked = false;
        var endpoint = CreateEndpoint(handler: context => { invoked = true; return Task.CompletedTask; });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, parent, arguments);

        Assert.True(result.IsError);
        Assert.False(invoked);
    }

    [Fact]
    public async Task InvokeEndpointAsync_PolicyDenied_DoesNotExecute()
    {
        var authentication = new Mock<IAuthenticationService>();
        authentication.Setup(service => service.ForbidAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Callback<HttpContext, string, AuthenticationProperties>((context, _, _) => context.Response.StatusCode = 403)
            .Returns(Task.CompletedTask);
        await using var services = CreateServices(authentication.Object);
        var parent = CreateContext(services);
        var invoked = false;
        var endpoint = CreateEndpoint(handler: context => { invoked = true; return Task.CompletedTask; },
            authorization: new AuthorizeAttribute("Denied"));
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");

        Assert.True(result.IsError);
        Assert.False(invoked);
        Assert.Contains("403", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    [Fact]
    public async Task InvokeEndpointAsync_HandlerFailure_RestoresContextAndHidesException()
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        var accessor = new HttpContextAccessor { HttpContext = parent };
        var endpoint = CreateEndpoint(handler: _ => throw new InvalidOperationException("secret database connection"));
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");

        Assert.True(result.IsError);
        Assert.Same(parent, accessor.HttpContext);
        Assert.DoesNotContain("secret", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    private static Task<CallToolResult> InvokeAsync(McpToolDefinition tool, HttpContext parent, string arguments) =>
        McpToolInvoker.InvokeEndpointAsync(tool, JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments), parent, NullLogger.Instance, CancellationToken.None);

    private static ServiceProvider CreateServices(IAuthenticationService authentication = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddPolicy("Denied", policy => policy.RequireAssertion(_ => false)));
        services.AddSingleton(authentication ?? Mock.Of<IAuthenticationService>());
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateContext(IServiceProvider services)
    {
        var context = new DefaultHttpContext { RequestServices = services, User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "alice")], "Bearer")) };
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com");
        context.Request.PathBase = "/tenant-a";
        return context;
    }

    private static RouteEndpoint CreateEndpoint(CliOperationMetadata metadata = null, RequestDelegate handler = null, IAuthorizeData authorization = null)
    {
        var items = new List<object>
        {
            metadata ?? new CliOperationMetadata(["widgets"], "update"),
            new EndpointNameMetadata("ApiUpdateWidget"),
            new HttpMethodMetadata(["PUT"]),
        };
        if (authorization is not null)
        {
            items.Add(authorization);
        }

        return new RouteEndpoint(handler ?? (_ => Task.CompletedTask), RoutePatternFactory.Parse("api/widgets/{id}"), 0, new EndpointMetadataCollection(items), "widget");
    }

    private static JsonNode Document() => JsonNode.Parse("""
        {
          "paths": {
            "/api/widgets/{id}": {
              "put": {
                "operationId": "ApiUpdateWidget",
                "description": "Updates a widget.",
                "parameters": [
                  {"name":"id","in":"path","required":true,"schema":{"type":"string"}},
                  {"name":"search","in":"query","schema":{"type":"string"}}
                ],
                "requestBody": {"required":true,"content":{"application/json":{"schema":{"$ref":"#/components/schemas/Widget"}}}}
              }
            }
          },
          "components": {"schemas": {
            "Widget":{"type":"object","properties":{"title":{"type":"string"},"child":{"$ref":"#/components/schemas/Widget"}}},
            "Unused":{"type":"string"}
          }}
        }
        """);
}
