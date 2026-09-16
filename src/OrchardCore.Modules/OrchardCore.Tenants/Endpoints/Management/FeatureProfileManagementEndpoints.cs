using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Endpoints.Management;

internal static class FeatureProfileManagementEndpoints
{
    internal const string CapabilityName = "tenant-feature-profiles";
    private const string Prefix = "api/tenants/feature-profiles";

    public static void AddFeatureProfileManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(Prefix, ListAsync), "ApiListTenantFeatureProfiles", "list", "Lists tenant feature-profile definitions.")
            .Produces<FeatureProfileListResponse>();
        Configure(routes.MapGet(Prefix + "/by-id", GetAsync), "ApiGetTenantFeatureProfile", "show", "Shows a feature profile by its immutable assignment identifier.", argument: true)
            .Produces<FeatureProfileDefinition>().ProducesProblem(404);
        Configure(routes.MapGet(Prefix + "/schema", SchemaAsync), "ApiGetTenantFeatureProfileSchema", "schema", "Gets the rule schema, including registered rule names.")
            .Produces<JsonElement>();
        Configure(routes.MapPost(Prefix, CreateAsync), "ApiCreateTenantFeatureProfile", "create", "Creates a feature profile; equivalent retries return its definition.", input: true)
            .Accepts<FeatureProfileDefinition>("application/json").Produces<FeatureProfileDefinition>(201).Produces<FeatureProfileDefinition>().ProducesProblem(409);
        Configure(routes.MapPut(Prefix + "/by-id", UpdateAsync), "ApiUpdateTenantFeatureProfile", "update", "Replaces a profile's display name and ordered rules without changing its identifier.", argument: true, input: true)
            .Accepts<FeatureProfileDefinition>("application/json").Produces<FeatureProfileDefinition>().ProducesProblem(404);
        Configure(routes.MapDelete(Prefix + "/by-id", DeleteAsync), "ApiDeleteTenantFeatureProfile", "delete", "Deletes a profile definition. Tenant assignments and enabled features are not changed.", argument: true, confirmation: true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string verb, string summary,
        bool argument = false, bool input = false, bool confirmation = false)
    {
        var metadata = new CliOperationMetadata(["tenants", "feature-profiles"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = confirmation,
        };
        if (argument) { metadata.Arguments.Add(new CliArgumentMetadata("id", 0)); }
        return builder.WithName(operationId).WithTags("Tenant Feature Profiles").WithSummary(summary)
            .WithCliCommand(metadata).DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(Permissions.ManageTenantFeatureProfiles)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] FeatureProfilesManager manager, [AsParameters] FeatureProfileListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200) { return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400); }
        var document = await manager.GetFeatureProfilesDocumentAsync();
        var matches = document.FeatureProfiles.Where(entry => string.IsNullOrWhiteSpace(request.Search) ||
            entry.Key.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
            (entry.Value.Name ?? entry.Key).Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToArray();
        return TypedResults.Ok(new FeatureProfileListResponse
        {
            Skip = skip, Take = take, TotalCount = matches.Length,
            Items = matches.Skip(skip).Take(take).Select(entry => Describe(entry.Key, entry.Value)).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] FeatureProfilesManager manager, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("A profile identifier is required.", statusCode: 400); }
        var document = await manager.GetFeatureProfilesDocumentAsync();
        return document.FeatureProfiles.TryGetValue(id, out var profile) ? TypedResults.Ok(Describe(id, profile)) : context.ApiNotFoundProblem();
    }

    internal static async Task<IResult> SchemaAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] IFeatureProfilesSchemaService schema)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok(JsonSerializer.Deserialize<JsonElement>(schema.GetJsonSchema()));
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] FeatureProfilesManager manager, [FromBody] FeatureProfileDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        if (RequiredDefinition(definition) is { } invalid) { return invalid; }
        var profile = ToProfile(definition);
        var document = await manager.GetFeatureProfilesDocumentAsync();
        if (document.FeatureProfiles.TryGetValue(definition.Id, out var current))
        {
            return FeatureProfilesManager.AreEquivalent(definition.Id, current, profile) ? TypedResults.Ok(Describe(definition.Id, current)) :
                TypedResults.Problem("A different definition already uses this profile identifier.", statusCode: 409);
        }
        if (await manager.ValidateFeatureProfileAsync(definition.Id, profile, isNew: true) is { Count: > 0 } errors)
        {
            return TypedResults.ValidationProblem(errors);
        }
        await manager.UpdateFeatureProfileAsync(definition.Id, profile);
        return TypedResults.Created($"{context.Request.PathBase}/{Prefix}/by-id?id={Uri.EscapeDataString(definition.Id)}", Describe(definition.Id, profile));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] FeatureProfilesManager manager, [FromQuery] string id, [FromBody] FeatureProfileDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        if (RequiredDefinition(definition) is { } invalid) { return invalid; }
        if (!string.Equals(id, definition.Id, StringComparison.OrdinalIgnoreCase)) { return TypedResults.Problem("The body identifier must match the query identifier.", statusCode: 400); }
        var document = await manager.GetFeatureProfilesDocumentAsync();
        if (!document.FeatureProfiles.ContainsKey(id)) { return context.ApiNotFoundProblem(); }
        var profile = ToProfile(definition);
        if (await manager.ValidateFeatureProfileAsync(id, profile) is { Count: > 0 } errors) { return TypedResults.ValidationProblem(errors); }
        await manager.UpdateFeatureProfileAsync(id, profile);
        return TypedResults.Ok(Describe(id, profile));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShellSettings shell, [FromServices] FeatureProfilesManager manager, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization, shell)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("A profile identifier is required.", statusCode: 400); }
        await manager.RemoveFeatureProfileAsync(id);
        return TypedResults.NoContent();
    }

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult RequiredDefinition(FeatureProfileDefinition definition) =>
        definition is null || string.IsNullOrWhiteSpace(definition.Id) || string.IsNullOrWhiteSpace(definition.Name)
        ? TypedResults.Problem("The profile identifier and display name are required.", statusCode: 400) : null;

    private static FeatureProfile ToProfile(FeatureProfileDefinition definition) => new()
    {
        Id = definition.Id, Name = definition.Name,
        FeatureRules = definition.FeatureRules?.Select(rule => rule is null ? null : new FeatureRule { Rule = rule.Rule, Expression = rule.Expression }).ToList(),
    };

    private static FeatureProfileDefinition Describe(string id, FeatureProfile profile) => new()
    {
        Id = id, Name = profile.Name ?? id,
        FeatureRules = profile.FeatureRules.Select(rule => new FeatureProfileRuleDefinition { Rule = rule.Rule, Expression = rule.Expression }).ToArray(),
    };

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization, ShellSettings shell) =>
        shell.IsDefaultShell() && await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, Permissions.ManageTenantFeatureProfiles);
}
