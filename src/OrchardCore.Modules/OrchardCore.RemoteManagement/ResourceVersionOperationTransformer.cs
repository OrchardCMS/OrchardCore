using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Declares the <c>ETag</c> response header and the optional <c>If-Match</c> request header of the operations marked with <see cref="ResourceVersionMetadata"/>.
/// </summary>
internal sealed class ResourceVersionOperationTransformer : IOpenApiOperationTransformer
{
    private const string ETagHeaderName = "ETag";
    private const string IfMatchHeaderName = "If-Match";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata.OfType<ResourceVersionMetadata>().LastOrDefault();
        if (metadata is null)
        {
            return Task.CompletedTask;
        }

        foreach (var (status, response) in operation.Responses ?? [])
        {
            // A 204 has no representation of the resource to version.
            if (status.StartsWith('2') && status != "204" && response is OpenApiResponse { } successResponse)
            {
                successResponse.Headers ??= new Dictionary<string, IOpenApiHeader>();
                successResponse.Headers[ETagHeaderName] = new OpenApiHeader
                {
                    Description = "The version of the resource, to send in the If-Match header of an update.",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                };
            }
        }

        if (metadata.AcceptsIfMatch)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = IfMatchHeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description = "The ETag of the version this update is based on, with its quotes. The update fails with 412 when the resource changed since. Without it, the update applies unconditionally.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
        }

        return Task.CompletedTask;
    }
}
