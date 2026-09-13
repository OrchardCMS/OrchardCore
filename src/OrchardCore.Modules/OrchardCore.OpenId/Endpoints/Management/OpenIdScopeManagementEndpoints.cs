using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.OpenId.Endpoints.Management;

internal static class OpenIdScopeManagementEndpoints
{
    public static void AddOpenIdScopeManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapPost("api/openid/scopes", CreateAsync), "ApiCreateOpenIdScope", "create",
            "Creates a scope; an equivalent name-based retry returns the existing scope.", input: true)
            .Produces<OpenIdScopeResponse>(201).Produces<OpenIdScopeResponse>().ProducesProblem(409);
        Configure(routes.MapPut("api/openid/scopes/by-name", UpdateAsync), "ApiUpdateOpenIdScope", "update",
            "Replaces a scope's editable fields while preserving extension properties.", input: true, argument: true)
            .Produces<OpenIdScopeResponse>().ProducesProblem(404);
        Configure(routes.MapDelete("api/openid/scopes/by-name", DeleteAsync), "ApiDeleteOpenIdScope", "delete",
            "Deletes a scope; an absent scope is a successful no-op. Existing tokens are not revoked.", argument: true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string id, string verb, string summary,
        bool input = false, bool argument = false)
    {
        var metadata = new CliOperationMetadata(["openid", "scopes"], verb)
        {
            Capability = OpenIdDiscoveryEndpoints.CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = verb == "delete",
        };
        if (argument) { metadata.Arguments.Add(new CliArgumentMetadata("name", 0)); }
        if (input) { builder.Accepts<OpenIdScopeMutationRequest>("application/json"); }
        return builder.WithName(id).WithTags("OpenID Management").WithSummary(summary).WithCliCommand(metadata)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(OpenIdPermissions.ManageScopes)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, OpenIdPermissions.ManageScopes);

    private static ValidationProblem Validate(OpenIdScopeMutationRequest request, ShellSettings settings)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request?.Name)) { errors["name"] = ["The scope name is required."]; }
        if (string.IsNullOrWhiteSpace(request?.DisplayName)) { errors["displayName"] = ["The display name is required."]; }
        if (request?.Resources is null || request.Resources.Any(resource => string.IsNullOrEmpty(resource) || resource.Contains(' ')))
        {
            errors["resources"] = ["Resources must be an array of nonempty individual identifiers without spaces."];
        }
        else if (OpenIdScopeEditor.ContainsCurrentTenantResource(string.Join(' ', request.Resources), settings.Name))
        {
            errors["resources"] = ["The resources field cannot contain the current tenant's reserved resource."];
        }
        return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdScopeManager manager, [FromServices] ShellSettings settings, [FromBody] OpenIdScopeMutationRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(request, settings) is { } invalid) { return invalid; }
        var ct = context.RequestAborted;
        var existing = await manager.FindByNameAsync(request.Name, ct);
        if (existing is not null)
        {
            return await OpenIdScopeEditor.MatchesAsync(manager, existing, request.Name, request.DisplayName, request.Description, request.Resources, ct)
                ? TypedResults.Ok(await OpenIdDiscoveryEndpoints.DescribeAsync(manager, existing, ct))
                : TypedResults.Problem("A scope with this name already exists with different editable fields.", statusCode: 409);
        }
        try
        {
            var result = await OpenIdScopeEditor.SaveAsync(manager, null, request.Name, request.DisplayName, request.Description, request.Resources, ct);
            return TypedResults.Created($"{context.Request.PathBase}/api/openid/scopes/by-name?name={Uri.EscapeDataString(request.Name)}",
                await OpenIdDiscoveryEndpoints.DescribeAsync(manager, result.Scope, ct));
        }
        catch (OpenIddictExceptions.ValidationException exception)
        {
            return ValidationProblem(exception);
        }
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdScopeManager manager, [FromServices] ShellSettings settings,
        [FromQuery] string name, [FromBody] OpenIdScopeMutationRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(request, settings) is { } invalid) { return invalid; }
        if (!string.Equals(name, request.Name, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The body name must match the query name. Renames are not supported.", statusCode: 400);
        }
        var ct = context.RequestAborted;
        var scope = await manager.FindByNameAsync(name, ct);
        if (scope is null) { return context.ApiNotFoundProblem(); }
        try
        {
            await OpenIdScopeEditor.SaveAsync(manager, scope, request.Name, request.DisplayName, request.Description, request.Resources, ct);
            return TypedResults.Ok(await OpenIdDiscoveryEndpoints.DescribeAsync(manager, scope, ct));
        }
        catch (OpenIddictExceptions.ValidationException exception)
        {
            return ValidationProblem(exception);
        }
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdScopeManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(name)) { return TypedResults.Problem("The scope name is required.", statusCode: 400); }
        var scope = await manager.FindByNameAsync(name, context.RequestAborted);
        if (scope is not null) { await manager.DeleteAsync(scope, context.RequestAborted); }
        return TypedResults.NoContent();
    }

    private static ValidationProblem ValidationProblem(OpenIddictExceptions.ValidationException exception) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["scope"] = exception.Results.Select(result => result.ErrorMessage).ToArray(),
        });
}
