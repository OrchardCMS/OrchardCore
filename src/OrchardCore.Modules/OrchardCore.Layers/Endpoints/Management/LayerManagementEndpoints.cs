using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Rules.Services;

namespace OrchardCore.Layers.Endpoints.Management;

internal static class LayerManagementEndpoints
{
    private const string RoutePrefix = "api/layers";
    internal const string CapabilityName = "layers";

    public static IEndpointRouteBuilder AddLayerManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(RoutePrefix, ListAsync), "ApiListLayers", "Lists layer definitions.", "list")
            .Produces<LayerListResponse>();
        Configure(routes.MapGet(RoutePrefix + "/by-name", GetAsync), "ApiGetLayer", "Shows a layer definition and its conditions.", "show", argument: true)
            .Produces<LayerDefinitionDto>().ProducesProblem(404);
        Configure(routes.MapGet("api/layer-conditions", ConditionsAsync), "ApiListLayerConditions", "Lists registered conditions and their writable property schemas.", "conditions")
            .Produces<IReadOnlyList<RuleConditionDescriptor>>();
        Configure(routes.MapPost(RoutePrefix + "/validate", ValidateAsync), "ApiValidateLayer", "Validates a complete layer definition without evaluating conditions or saving it.", "validate", input: true)
            .Accepts<LayerDefinitionDto>("application/json").Produces<RuleValidationResponse>();
        Configure(routes.MapPost(RoutePrefix, CreateAsync), "ApiCreateLayer", "Creates a complete layer definition; identical name retries return the stored definition.", "create", input: true)
            .Accepts<LayerDefinitionDto>("application/json").Produces<LayerDefinitionDto>(201).ProducesProblem(409);
        Configure(routes.MapPut(RoutePrefix + "/by-name", UpdateAsync), "ApiUpdateLayer", "Replaces a layer definition and all its conditions. The name cannot change.", "update", argument: true, input: true)
            .Accepts<LayerDefinitionDto>("application/json").Produces<LayerDefinitionDto>().ProducesProblem(404);
        Configure(routes.MapDelete(RoutePrefix + "/by-name", DeleteAsync), "ApiDeleteLayer", "Deletes an unreferenced layer; missing layers are a successful no-op.", "delete", argument: true, confirmation: true)
            .Produces(204).ProducesProblem(409);
        return routes;
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string summary,
        string verb, bool argument = false, bool input = false, bool confirmation = false)
    {
        var metadata = new CliOperationMetadata(["layers"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = confirmation,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("name", 0));
        }
        return builder.WithName(operationId).WithTags("Layers").WithSummary(summary)
            .WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(Permissions.ManageLayers)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [FromServices] IRuleManagementService rules, [AsParameters] LayerListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200)
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var document = await layers.GetLayersAsync();
        var matches = document.Layers.Where(layer => string.IsNullOrWhiteSpace(request.Search)
            || layer.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
            || (layer.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(layer => layer.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        return TypedResults.Ok(new LayerListResponse
        {
            Skip = skip, Take = take, TotalCount = matches.Length,
            Items = matches.Skip(skip).Take(take).Select(layer => Describe(layer, rules)).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [FromServices] IRuleManagementService rules, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var layer = await layers.GetLayerAsync(name);
        return layer is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(layer, rules));
    }

    internal static async Task<IResult> ConditionsAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRuleManagementService rules) => !await AuthorizedAsync(context, authorization)
        ? context.ApiForbidProblem() : TypedResults.Ok(rules.GetDescriptors());

    internal static async Task<IResult> ValidateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRuleManagementService rules, [FromServices] ILayerService layers, [FromBody] LayerDefinitionDto definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var result = Validate(definition, layers, rules);
        return TypedResults.Ok(new RuleValidationResponse { IsValid = result.IsValid, Errors = result.Errors });
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [FromServices] IRuleManagementService rules, [FromBody] LayerDefinitionDto definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var result = Validate(definition, layers, rules);
        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        var mutation = await layers.CreateAsync(definition.Name, definition.Description, result.Rule);
        if (mutation.Status == LayerMutationStatus.Conflict)
        {
            var existing = mutation.Layer;
            return Equivalent(Describe(existing, rules), definition, rules.Describe(result.Rule))
                ? TypedResults.Created(Location(context, existing.Name), Describe(existing, rules))
                : TypedResults.Problem("A layer with this name already exists with a different definition.", statusCode: 409);
        }
        return TypedResults.Created(Location(context, mutation.Layer.Name), Describe(mutation.Layer, rules));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [FromServices] IRuleManagementService rules, [FromQuery] string name, [FromBody] LayerDefinitionDto definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        if (definition is null || !string.Equals(name, definition.Name, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The body name must match the name query parameter. Layer renames are not supported.", statusCode: 400);
        }
        var existing = await layers.GetLayerAsync(name);
        if (existing is null)
        {
            return context.ApiNotFoundProblem();
        }
        var result = Validate(definition, layers, rules, existing.LayerRule?.ConditionId);
        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        // Preserve generated condition IDs and avoid a document mutation on an identical retry.
        if (!Equivalent(Describe(existing, rules), definition, rules.Describe(result.Rule)))
        {
            var mutation = await layers.UpdateAsync(name, definition.Description, result.Rule);
            if (mutation.Status == LayerMutationStatus.NotFound)
            {
                return context.ApiNotFoundProblem();
            }
            existing = mutation.Layer;
        }
        return TypedResults.Ok(Describe(existing, rules));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var result = await layers.DeleteAsync(name);
        return result.Status == LayerMutationStatus.Referenced
            ? TypedResults.Problem("Remove or move the associated widgets before deleting this layer.", statusCode: 409)
            : TypedResults.NoContent();
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, Permissions.ManageLayers);

    private static RuleManagementResult Validate(LayerDefinitionDto definition, ILayerService layers, IRuleManagementService rules, string ruleId = null)
    {
        if (layers.ValidateName(definition?.Name) is { } error)
        {
            return new RuleManagementResult { Errors = new Dictionary<string, string[]> { ["name"] = [error] } };
        }
        return rules.CreateRule(definition.Conditions, ruleId);
    }

    private static LayerDefinitionDto Describe(Layer layer, IRuleManagementService rules) =>
        new() { Name = layer.Name, Description = layer.Description, Conditions = rules.Describe(layer.LayerRule) };

    private static bool Equivalent(LayerDefinitionDto existing, LayerDefinitionDto requested, IReadOnlyList<RuleConditionDefinition> normalized) =>
        string.Equals(existing.Name, requested.Name, StringComparison.OrdinalIgnoreCase)
        && existing.Description == requested.Description && ConditionsEqual(existing.Conditions, requested.Conditions, normalized);

    private static bool ConditionsEqual(IReadOnlyList<RuleConditionDefinition> existing, IReadOnlyList<RuleConditionDefinition> requested,
        IReadOnlyList<RuleConditionDefinition> normalized) => existing.Count == requested.Count
        && Enumerable.Range(0, existing.Count).All(index => existing[index].Name == requested[index].Name
            && (string.IsNullOrEmpty(requested[index].ConditionId) || existing[index].ConditionId == requested[index].ConditionId)
            && JsonNode.DeepEquals(existing[index].Properties, normalized[index].Properties)
            && ConditionsEqual(existing[index].Conditions, requested[index].Conditions, normalized[index].Conditions));

    private static string Location(HttpContext context, string name) =>
        $"{context.Request.PathBase}/{RoutePrefix}/by-name?name={Uri.EscapeDataString(name)}";
}

/// <summary>Reports layer definition validation without mutation or condition execution.</summary>
public sealed class RuleValidationResponse
{
    /// <summary>Gets whether the complete definition is valid.</summary>
    public bool IsValid { get; init; }
    /// <summary>Gets validation errors keyed by request property path.</summary>
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
