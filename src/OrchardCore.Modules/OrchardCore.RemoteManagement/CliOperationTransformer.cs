using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrchardCore.RemoteManagement;

internal sealed class CliOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<CliOperationMetadata>()
            .LastOrDefault();

        if (metadata is null)
        {
            return Task.CompletedTask;
        }

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions[RemoteManagementConstants.CliExtensionName] =
            new JsonNodeExtension(CreateExtension(metadata));

        return Task.CompletedTask;
    }

    private static JsonObject CreateExtension(CliOperationMetadata metadata)
    {
        var extension = new JsonObject
        {
            ["commandGroup"] = new JsonArray(metadata.CommandGroup.Select(segment => JsonValue.Create(segment)).ToArray()),
            ["verb"] = metadata.Verb,
            ["inputMode"] = metadata.InputMode.ToString().ToLowerInvariant(),
            ["requiresConfirmation"] = metadata.RequiresConfirmation,
            ["hidden"] = metadata.Hidden,
        };

        if (!string.IsNullOrEmpty(metadata.Capability))
        {
            extension["capability"] = metadata.Capability;
        }

        if (!string.IsNullOrWhiteSpace(metadata.DefaultJsonBody))
        {
            extension["defaultJsonBody"] = metadata.DefaultJsonBody;
        }

        if (metadata.Aliases.Count > 0)
        {
            extension["aliases"] = new JsonArray(metadata.Aliases.Select(alias => JsonValue.Create(alias)).ToArray());
        }

        if (metadata.Arguments.Count > 0)
        {
            extension["arguments"] = new JsonArray(metadata.Arguments
                .OrderBy(argument => argument.Position)
                .Select(argument => new JsonObject
                {
                    ["parameterName"] = argument.ParameterName,
                    ["position"] = argument.Position,
                })
                .ToArray());
        }

        if (metadata.TableColumns.Count > 0)
        {
            extension["tableColumns"] = new JsonArray(metadata.TableColumns
                .Select(column => new JsonObject
                {
                    ["propertyPath"] = column.PropertyPath,
                    ["heading"] = column.Heading,
                })
                .ToArray());
        }

        if (metadata.SecretProperties.Count > 0)
        {
            extension["secretProperties"] = new JsonArray(metadata.SecretProperties
                .Select(property => JsonValue.Create(property))
                .ToArray());
        }

        if (metadata.FileResponse)
        {
            extension["fileResponse"] = true;
        }

        if (metadata.SecretResponse)
        {
            extension["secretResponse"] = true;
        }

        return extension;
    }
}
