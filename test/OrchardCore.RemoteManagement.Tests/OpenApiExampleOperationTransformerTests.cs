using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;

namespace OrchardCore.RemoteManagement.Tests;

public class OpenApiExampleOperationTransformerTests
{
    [Fact]
    public async Task TransformAsync_Examples_SetsThemOnJsonBodiesOfSuccessfulResponses()
    {
        var operation = CreateOperation();

        await TransformAsync(operation, new OpenApiExampleMetadata("""{"name":"request"}""", """{"name":"response"}"""));

        Assert.Equal("request", operation.RequestBody.Content["application/json"].Example["name"].GetValue<string>());
        Assert.Equal("response", operation.Responses["201"].Content["application/json"].Example["name"].GetValue<string>());
        Assert.Equal("response", operation.Responses["200"].Content["application/problem+json"].Example["name"].GetValue<string>());
        Assert.Null(operation.Responses["200"].Content["text/plain"].Example);
        Assert.Null(operation.Responses["400"].Content["application/problem+json"].Example);
    }

    [Fact]
    public async Task TransformAsync_DocumentGeneratedTwice_DoesNotShareNodes()
    {
        var metadata = new OpenApiExampleMetadata("""{"name":"request"}""", """{"name":"response"}""");
        var first = CreateOperation();
        var second = CreateOperation();

        await TransformAsync(first, metadata);
        await TransformAsync(second, metadata);

        Assert.NotSame(first.RequestBody.Content["application/json"].Example, second.RequestBody.Content["application/json"].Example);
        Assert.NotSame(first.Responses["201"].Content["application/json"].Example, first.Responses["200"].Content["application/problem+json"].Example);
    }

    [Fact]
    public async Task TransformAsync_ResponseExampleOnly_LeavesTheRequestUntouched()
    {
        var operation = CreateOperation();

        await TransformAsync(operation, new OpenApiExampleMetadata(null, "[]"));

        Assert.Null(operation.RequestBody.Content["application/json"].Example);
        Assert.IsType<JsonArray>(operation.Responses["201"].Content["application/json"].Example);
    }

    [Fact]
    public async Task TransformAsync_OperationWithoutBodyOrResponses_DoesNothing()
    {
        var operation = new OpenApiOperation();

        await TransformAsync(operation, new OpenApiExampleMetadata("{}", "{}"));

        Assert.Null(operation.RequestBody);
        Assert.Empty(operation.Responses);
    }

    [Fact]
    public async Task TransformAsync_NoMetadata_DoesNothing()
    {
        var operation = CreateOperation();

        await TransformAsync(operation);

        Assert.Null(operation.RequestBody.Content["application/json"].Example);
        Assert.Null(operation.Responses["201"].Content["application/json"].Example);
    }

    [Fact]
    public void WithOpenApiExample_InvalidJson_FailsWhenTheEndpointIsMapped()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        Assert.ThrowsAny<JsonException>(() => app.MapGet("example", () => "value").WithOpenApiExample(response: "{"));
    }

    private static OpenApiOperation CreateOperation() => new()
    {
        RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() },
        },
        Responses = new OpenApiResponses
        {
            ["201"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new() } },
            ["200"] = new OpenApiResponse
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new(),
                    ["text/plain"] = new(),
                },
            },
            ["400"] = new OpenApiResponse { Content = new Dictionary<string, OpenApiMediaType> { ["application/problem+json"] = new() } },
        },
    };

    private static async Task TransformAsync(OpenApiOperation operation, params object[] metadata)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var context = new OpenApiOperationTransformerContext
        {
            DocumentName = "v1",
            Description = new ApiDescription { ActionDescriptor = new ActionDescriptor { EndpointMetadata = metadata } },
            ApplicationServices = services,
        };

        await new OpenApiExampleOperationTransformer().TransformAsync(operation, context, TestContext.Current.CancellationToken);
    }
}
