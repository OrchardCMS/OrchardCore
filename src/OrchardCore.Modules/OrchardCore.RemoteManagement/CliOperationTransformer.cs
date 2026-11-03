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

        if (metadata.LongRunning)
        {
            extension["longRunning"] = true;
        }

        if (metadata.FileResponse)
        {
            extension["fileResponse"] = true;
        }

        if (metadata.SecretResponse)
        {
            extension["secretResponse"] = true;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ProvisioningProperty))
        {
            extension["provisioningProperty"] = metadata.ProvisioningProperty;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ResourceName))
        {
            extension["resourceName"] = metadata.ResourceName;
        }

        if (!string.IsNullOrWhiteSpace(metadata.CompletedAction))
        {
            extension["completedAction"] = metadata.CompletedAction;
        }

        if (metadata.NextSteps.Count > 0)
        {
            extension["nextSteps"] = new JsonArray(metadata.NextSteps.Select(CreateNextStep).ToArray());
        }

        if (metadata.SummaryProperties.Count > 0)
        {
            extension["summaryProperties"] = new JsonArray(metadata.SummaryProperties
                .Select(property => JsonValue.Create(property))
                .ToArray());
        }

        if (!string.IsNullOrWhiteSpace(metadata.ItemsProperty))
        {
            extension["itemsProperty"] = metadata.ItemsProperty;
        }

        if (!string.IsNullOrWhiteSpace(metadata.SetupPasswordProperty))
        {
            extension["setupPasswordProperty"] = metadata.SetupPasswordProperty;
        }

        return extension;
    }

    private static JsonObject CreateNextStep(CliNextStepMetadata step)
    {
        var extension = new JsonObject();

        if (!string.IsNullOrWhiteSpace(step.Description))
        {
            extension["description"] = step.Description;
        }

        if (!string.IsNullOrWhiteSpace(step.WhenProperty))
        {
            extension["whenProperty"] = step.WhenProperty;
            extension["whenValue"] = step.WhenValue;
        }

        if (step.Arguments.Count > 0)
        {
            extension["arguments"] = CreateStringArray(step.Arguments);
        }

        if (step.OptionalArguments.Count > 0)
        {
            extension["optionalArguments"] = new JsonArray(step.OptionalArguments
                .Select(optional => new JsonObject
                {
                    ["whenMissing"] = optional.WhenMissing,
                    ["arguments"] = CreateStringArray(optional.Arguments),
                })
                .ToArray());
        }

        if (!string.IsNullOrWhiteSpace(step.Note))
        {
            extension["note"] = step.Note;
        }

        if (step.SavesContext)
        {
            extension["savesContext"] = true;
        }

        return extension;
    }

    private static JsonArray CreateStringArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
