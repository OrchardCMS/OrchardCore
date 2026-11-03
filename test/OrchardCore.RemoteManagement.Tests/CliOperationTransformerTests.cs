using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;

namespace OrchardCore.RemoteManagement.Tests;

public class CliOperationTransformerTests
{
    [Fact]
    public async Task TransformAsync_PresentationMetadata_EmitsIt()
    {
        var extension = await TransformAsync(new CliOperationMetadata(["tenants"], "create")
        {
            CompletedAction = "launched",
            LongRunning = true,
            ReadOnly = true,
            RequestSchemaPath = "api/tenants/{name}/schema",
            NextSteps =
            {
                new CliNextStepMetadata
                {
                    Description = "start this tenant.",
                    WhenProperty = "state",
                    WhenValue = "Disabled",
                    Arguments = { "tenants", "start", "{name}" },
                    OptionalArguments = { new CliNextStepOptionalArguments("recipeName", "--recipe-name", "<recipe-name>") },
                    Note = "Read this.",
                },
                new CliNextStepMetadata { SavesContext = true },
            },
        });

        Assert.Equal("launched", extension["completedAction"]!.GetValue<string>());
        Assert.True(extension["longRunning"]!.GetValue<bool>());
        Assert.True(extension["readOnly"]!.GetValue<bool>());
        Assert.Equal("api/tenants/{name}/schema", extension["requestSchemaPath"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""
                [{"description":"start this tenant.","whenProperty":"state","whenValue":"Disabled","arguments":["tenants","start","{name}"],
                  "optionalArguments":[{"whenMissing":"recipeName","arguments":["--recipe-name","<recipe-name>"]}],"note":"Read this."},
                 {"savesContext":true}]
                """),
            extension["nextSteps"]));
    }

    [Fact]
    public async Task TransformAsync_NoPresentationMetadata_LeavesItOut()
    {
        var extension = await TransformAsync(new CliOperationMetadata(["tenants"], "show"));

        Assert.False(extension.ContainsKey("completedAction"));
        Assert.False(extension.ContainsKey("longRunning"));
        Assert.False(extension.ContainsKey("readOnly"));
        Assert.False(extension.ContainsKey("requestSchemaPath"));
        Assert.False(extension.ContainsKey("nextSteps"));
    }

    private static async Task<JsonObject> TransformAsync(CliOperationMetadata metadata)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var operation = new OpenApiOperation();
        var context = new OpenApiOperationTransformerContext
        {
            DocumentName = "v1",
            Description = new ApiDescription { ActionDescriptor = new ActionDescriptor { EndpointMetadata = [metadata] } },
            ApplicationServices = services,
        };

        await new CliOperationTransformer().TransformAsync(operation, context, TestContext.Current.CancellationToken);

        return Assert.IsType<JsonObject>(Assert.IsType<JsonNodeExtension>(operation.Extensions![RemoteManagementConstants.CliExtensionName]).Node);
    }
}
