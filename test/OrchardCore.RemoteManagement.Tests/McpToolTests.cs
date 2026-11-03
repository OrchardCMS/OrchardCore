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
using OrchardCore.Environment.Shell;
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
    public void CreateTools_ExamplesInTheDocument_LeaveTheToolUnchanged()
    {
        var expected = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], Document()));
        var document = Document();
        var operation = document["paths"]["/api/widgets/{id}"]["put"];
        operation["requestBody"]["content"]["application/json"]["example"] = JsonNode.Parse("""{"title":"Example","password":"Replace-Me-1"}""");
        operation["responses"] = JsonNode.Parse("""{"200":{"content":{"application/json":{"example":{"title":"Example"}}}}}""");

        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], document));

        Assert.Equal(expected.Tool.InputSchema.GetRawText(), tool.Tool.InputSchema.GetRawText());
        Assert.Equal(expected.Tool.Description, tool.Tool.Description);
        Assert.DoesNotContain("Replace-Me-1", tool.Tool.InputSchema.GetRawText(), StringComparison.Ordinal);
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
        var accessor = new McpHttpContextAccessor { HttpContext = parent };
        var hostAccessor = new HttpContextAccessor();
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
        Assert.Same(dispatched, ambient);
        Assert.Equal("/tenant-a", dispatched.Request.PathBase.Value);
        Assert.Equal("a/b?c", dispatched.Request.RouteValues["id"]);
        Assert.Equal("one&two", dispatched.Request.Query["search"]);
        Assert.Equal("{\"title\":\"Example\"}", inputBody);
        Assert.False(result.IsError == true);
        Assert.Same(parent, accessor.HttpContext);
        Assert.Same(parent, hostAccessor.HttpContext);
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
        var accessor = new McpHttpContextAccessor { HttpContext = parent };
        var endpoint = CreateEndpoint(handler: _ => throw new InvalidOperationException("secret database connection"));
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");

        Assert.True(result.IsError);
        Assert.Same(parent, accessor.HttpContext);
        Assert.DoesNotContain("secret", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    [Fact]
    public void RequiresBearerToken_ApiSchemeWithoutAnonymousAccess_ReturnsTrue()
    {
        var api = new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api };
        var policy = new AuthorizationPolicyBuilder(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser().Build();

        Assert.True(ManagementSecurityOperationTransformer.RequiresBearerToken([api]));
        Assert.True(ManagementSecurityOperationTransformer.RequiresBearerToken([policy]));
        Assert.False(ManagementSecurityOperationTransformer.RequiresBearerToken([policy, new AllowAnonymousAttribute()]));
        Assert.False(ManagementSecurityOperationTransformer.RequiresBearerToken([new AuthorizeAttribute()]));
        Assert.False(ManagementSecurityOperationTransformer.RequiresBearerToken([new CliOperationMetadata(["widgets"], "show")]));
    }

    [Fact]
    public void AllowsOptionalBearerToken_AnonymousCliOperation_ReturnsTrue()
    {
        var policy = new AuthorizationPolicyBuilder(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser().Build();
        var cli = new CliOperationMetadata(["widgets"], "show");

        Assert.True(ManagementSecurityOperationTransformer.AllowsOptionalBearerToken([cli, new AllowAnonymousAttribute()]));
        Assert.False(ManagementSecurityOperationTransformer.AllowsOptionalBearerToken([cli, policy]));
        Assert.False(ManagementSecurityOperationTransformer.AllowsOptionalBearerToken([new AllowAnonymousAttribute()]));
    }

    [Fact]
    public void CreateTools_OptionalIfMatchHeader_KeepsTheToolWithoutTheHeader()
    {
        var document = Document();
        document["paths"]["/api/widgets/{id}"]["put"]["parameters"].AsArray().Add(JsonNode.Parse("""{"name":"If-Match","in":"header","schema":{"type":"string"}}"""));

        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], document));

        Assert.DoesNotContain("If-Match", tool.Tool.InputSchema.GetRawText());
    }

    [Fact]
    public void CreateTools_OtherHeaderParameter_ExcludesTheTool()
    {
        var document = Document();
        document["paths"]["/api/widgets/{id}"]["put"]["parameters"].AsArray().Add(JsonNode.Parse("""{"name":"X-Custom","in":"header","schema":{"type":"string"}}"""));

        Assert.Empty(McpToolCatalog.CreateTools([CreateEndpoint()], document));
    }

    private static Task<CallToolResult> InvokeAsync(McpToolDefinition tool, HttpContext parent, string arguments) =>
        McpToolInvoker.InvokeEndpointAsync(tool, JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments), parent, NullLogger.Instance, CancellationToken.None);

    [Fact]
    public async Task InvokeEndpointAsync_BackgroundFlowOutlivesTheCall_DoesNotKeepTheDispatchedRequest()
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        var accessor = new McpHttpContextAccessor { HttpContext = parent };
        var callCompleted = new TaskCompletionSource();
        Task<HttpContext> background = null;
        var endpoint = CreateEndpoint(handler: _ =>
        {
            background = Task.Run(async () =>
            {
                await callCompleted.Task;
                return accessor.HttpContext;
            });
            return Task.CompletedTask;
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");
        callCompleted.SetResult();

        Assert.Same(parent, await background);
    }

    [Fact]
    public async Task InvokeEndpointAsync_DispatchedRequest_KeepsTheConnectionAndTenantOfTheCaller()
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        parent.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("203.0.113.1");
        var feature = new ShellContextFeature { OriginalPathBase = "/root" };
        parent.Features.Set(feature);
        HttpContext dispatched = null;
        var endpoint = CreateEndpoint(handler: context =>
        {
            dispatched = context;
            return Task.CompletedTask;
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");

        Assert.Equal(parent.Connection.RemoteIpAddress, dispatched.Connection.RemoteIpAddress);
        Assert.Same(feature, dispatched.Features.Get<ShellContextFeature>());
    }

    [Theory]
    [InlineData(1024, false)]
    [InlineData(4096, false)]
    [InlineData(4097, true)]
    [InlineData(1024 * 1024, true)]
    public async Task InvokeEndpointAsync_ResponseOverTheLimit_ReturnsAToolError(int length, bool rejected)
    {
        await using var services = CreateServices();
        var endpoint = CreateEndpoint(handler: async context =>
        {
            context.Response.ContentType = "text/plain";
            var chunk = new string('x', 1024);
            for (var written = 0; written < length; written += chunk.Length)
            {
                await context.Response.WriteAsync(chunk[..Math.Min(chunk.Length, length - written)]);
                await context.Response.Body.FlushAsync();
            }
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await McpToolInvoker.InvokeEndpointAsync(tool, JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{"path":{"id":"a"},"body":{}}"""),
            CreateContext(services), NullLogger.Instance, CancellationToken.None, 4096);

        Assert.Equal(rejected, result.IsError == true);
        Assert.Equal(rejected, Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text.Contains("4096 bytes"));
    }

    [Fact]
    public async Task InvokeEndpointAsync_EndpointHandlesTheLimitException_StillReturnsAToolError()
    {
        await using var services = CreateServices();
        var endpoint = CreateEndpoint(handler: async context =>
        {
            try
            {
                await context.Response.Body.WriteAsync(new byte[8192]);
            }
            catch (Exception)
            {
                context.Response.StatusCode = 200;
            }
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await McpToolInvoker.InvokeEndpointAsync(tool, JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{"path":{"id":"a"},"body":{}}"""),
            CreateContext(services), NullLogger.Instance, CancellationToken.None, 4096);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task InvokeEndpoint_JsonResponse_IsEmbeddedInResult()
    {
        await using var services = CreateServices();
        var endpoint = CreateEndpoint(handler: context => context.Response.WriteAsync("{\"saved\":\"it's\"}"));
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, CreateContext(services), """{"path":{"id":"a"},"body":{}}""");

        Assert.Equal("""{"statusCode":200,"body":{"saved":"it's"}}""", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    [Fact]
    public async Task InvokeEndpoint_TextResponse_IsReturnedAsString()
    {
        await using var services = CreateServices();
        var endpoint = CreateEndpoint(handler: context => context.Response.WriteAsync("not json"));
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        var result = await InvokeAsync(tool, CreateContext(services), """{"path":{"id":"a"},"body":{}}""");

        Assert.Equal("""{"statusCode":200,"body":"not json"}""", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }

    [Theory]
    [InlineData(nameof(CliOperationMetadata.SecretResponse))]
    [InlineData(nameof(CliOperationMetadata.SecretProperties))]
    [InlineData(nameof(CliOperationMetadata.ExcludedFromMcp))]
    public void CreateTools_CredentialOperation_IsExcluded(string flag)
    {
        var metadata = new CliOperationMetadata(["widgets"], "update")
        {
            SecretResponse = flag == nameof(CliOperationMetadata.SecretResponse),
            ExcludedFromMcp = flag == nameof(CliOperationMetadata.ExcludedFromMcp),
        };
        if (flag == nameof(CliOperationMetadata.SecretProperties))
        {
            metadata.SecretProperties.Add("password");
        }

        Assert.Empty(McpToolCatalog.CreateTools([CreateEndpoint(metadata)], Document()));
    }

    [Fact]
    public void CreateTools_DocumentDeclaringSecurity_IsUnaffected()
    {
        var document = Document();
        document["components"]["securitySchemes"] = JsonNode.Parse("""{"Bearer":{"type":"http","scheme":"bearer"}}""");
        document["paths"]["/api/widgets/{id}"]["put"]["security"] = JsonNode.Parse("""[{"Bearer":[]}]""");

        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], document));

        Assert.Equal(
            Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint()], Document())).Tool.InputSchema.GetRawText(),
            tool.Tool.InputSchema.GetRawText());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void CreateTools_ConfirmationRequired_IsExcludedUnlessAllowed(bool? allowed, bool expected)
    {
        var metadata = new CliOperationMetadata(["widgets"], "update") { RequiresConfirmation = true };
        var options = allowed is null ? null : new McpToolOptions { AllowConfirmationRequiredTools = allowed.Value };

        var tools = McpToolCatalog.CreateTools([CreateEndpoint(metadata)], Document(), options);

        Assert.Equal(expected, tools.Count == 1);
    }

    [Fact]
    public void CreateTools_SecretOperation_StaysExcludedWhenConfirmationIsAllowed()
    {
        var metadata = new CliOperationMetadata(["widgets"], "update") { RequiresConfirmation = true, SecretResponse = true };

        Assert.Empty(McpToolCatalog.CreateTools([CreateEndpoint(metadata)], Document(), new McpToolOptions { AllowConfirmationRequiredTools = true }));
    }

    [Theory]
    [InlineData("""{"writeOnly":true}""", true)]
    [InlineData("""{"type":"array","items":{"type":"object","properties":{"key":{"writeOnly":true}}}}""", true)]
    [InlineData("""{"writeOnly":false}""", false)]
    [InlineData("""{"type":"string"}""", false)]
    public void CreateTools_WriteOnlyBodyProperty_IsExcluded(string property, bool excluded)
    {
        var document = Document();
        document["components"]["schemas"]["Widget"]["properties"]["secret"] = JsonNode.Parse(property);

        var tools = McpToolCatalog.CreateTools([CreateEndpoint()], document);

        Assert.Equal(excluded ? 0 : 1, tools.Count);
    }

    [Fact]
    public async Task InvokeEndpointAsync_AnyTool_MarksTheDispatchedRequest()
    {
        await using var services = CreateServices();
        var parent = CreateContext(services);
        var marked = false;
        var endpoint = CreateEndpoint(handler: context =>
        {
            marked = context.IsMcpToolCall();
            return Task.CompletedTask;
        });
        var tool = Assert.Single(McpToolCatalog.CreateTools([endpoint], Document()));

        await InvokeAsync(tool, parent, """{"path":{"id":"a"},"body":{}}""");

        Assert.True(marked);
        Assert.False(parent.IsMcpToolCall());
    }

    [Theory]
    [InlineData("create", false, false)]
    [InlineData("create", true, true)]
    [InlineData("update", false, true)]
    public void CreateTools_DestructiveHint_ExcludesPlainCreation(string verb, bool requiresConfirmation, bool expected)
    {
        var metadata = new CliOperationMetadata(["widgets"], verb) { RequiresConfirmation = requiresConfirmation };

        var tool = Assert.Single(McpToolCatalog.CreateTools([CreateEndpoint(metadata)], Document(), new McpToolOptions { AllowConfirmationRequiredTools = true }));

        Assert.Equal(expected, tool.Tool.Annotations.DestructiveHint);
    }

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
