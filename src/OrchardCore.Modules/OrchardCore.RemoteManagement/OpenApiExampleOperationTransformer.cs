using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Sets the request and response examples of the operations marked with <see cref="OpenApiExampleMetadata"/>.
/// </summary>
internal sealed class OpenApiExampleOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata.OfType<OpenApiExampleMetadata>().LastOrDefault();
        if (metadata is null)
        {
            return Task.CompletedTask;
        }

        if (metadata.Request is not null)
        {
            SetExample(operation.RequestBody?.Content, metadata.Request);
        }

        if (metadata.Response is not null)
        {
            foreach (var (status, response) in operation.Responses ?? [])
            {
                if (status.StartsWith('2'))
                {
                    SetExample(response.Content, metadata.Response);
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void SetExample(IDictionary<string, OpenApiMediaType> content, JsonNode example)
    {
        if (content is null)
        {
            return;
        }

        foreach (var (mediaType, description) in content)
        {
            if (mediaType == "application/json" || mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
            {
                // A node has a single parent, and the document can be generated many times.
                description.Example = example.DeepClone();
            }
        }
    }
}
