using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.OpenId.Endpoints.Management;

internal static class OpenIdApplicationManagementEndpoints
{
    public static void AddOpenIdApplicationManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapPost("api/openid/applications", CreateAsync), "ApiCreateOpenIdApplication", "create",
            "Creates an application. Equivalent client-identifier retries return its redacted description.", input: true)
            .Produces<OpenIdApplicationResponse>(201).Produces<OpenIdApplicationResponse>().ProducesProblem(409);
        Configure(routes.MapPut("api/openid/applications/by-client-id", UpdateAsync), "ApiUpdateOpenIdApplication", "update",
            "Replaces application settings. An omitted secret preserves the current confidential credential.", input: true, argument: true)
            .Produces<OpenIdApplicationResponse>().ProducesProblem(404);
        Configure(routes.MapDelete("api/openid/applications/by-client-id", DeleteAsync), "ApiDeleteOpenIdApplication", "delete",
            "Deletes an application. An absent application is a successful no-op; issued token lifetimes still apply.", argument: true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string id, string verb, string summary,
        bool input = false, bool argument = false)
    {
        var metadata = new CliOperationMetadata(["openid", "applications"], verb)
        {
            Capability = OpenIdDiscoveryEndpoints.CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = verb == "delete",
        };
        if (argument) { metadata.Arguments.Add(new CliArgumentMetadata("clientId", 0)); }
        if (input)
        {
            builder.Accepts<OpenIdApplicationMutationRequest>("application/json");
            metadata.SecretProperties.Add("clientSecret");
        }
        return builder.WithName(id).WithTags("OpenID Management").WithSummary(summary).WithCliCommand(metadata)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(OpenIdPermissions.ManageApplications)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, OpenIdPermissions.ManageApplications);

    private static ValidationProblem Validate(OpenIdApplicationMutationRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request?.ClientId)) { errors["clientId"] = ["The client identifier is required."]; }
        if (string.IsNullOrWhiteSpace(request?.DisplayName)) { errors["displayName"] = ["The display name is required."]; }
        if (request?.ClientType is not ("public" or "confidential")) { errors["clientType"] = ["Use public or confidential."]; }
        if (request?.ApplicationType is not ("web" or "native")) { errors["applicationType"] = ["Use web or native."]; }
        if (request?.ConsentType is not ("explicit" or "implicit" or "external" or "systematic")) { errors["consentType"] = ["Use explicit, implicit, external or systematic."]; }
        if (request?.Roles is null || request.Roles.Any(string.IsNullOrWhiteSpace)) { errors["roles"] = ["Roles must be an array of nonempty names."]; }
        if (request?.Scopes is null || request.Scopes.Any(string.IsNullOrWhiteSpace)) { errors["scopes"] = ["Scopes must be an array of nonempty names."]; }
        return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
    }

    private static async Task<ValidationProblem> ValidateReferencesAsync(HttpContext context, IOpenIdScopeManager scopes,
        OpenIdApplicationMutationRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.Roles.Length > 0)
        {
            var roles = context.RequestServices.GetService<IRoleService>();
            var names = roles is null ? [] : (await roles.GetRoleNamesAsync()).ToArray();
            if (request.Roles.Except(names, StringComparer.Ordinal).Any())
            {
                errors["roles"] = ["Every role must be a registered role name in this tenant."];
            }
        }
        foreach (var name in request.Scopes.Distinct(StringComparer.Ordinal))
        {
            if (name is not (OpenIddictConstants.Scopes.OpenId or OpenIddictConstants.Scopes.OfflineAccess) &&
                await scopes.FindByNameAsync(name, context.RequestAborted) is null)
            {
                errors["scopes"] = ["Every scope must be registered in this tenant, or be openid or offline_access."];
                break;
            }
        }
        return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
    }

    private static async Task<ValidationProblem> ValidateClientAsync(HttpContext context, IOpenIdApplicationManager manager,
        OpenIdApplicationMutationRequest request, object application, bool isNew, CancellationToken cancellationToken)
    {
        var errors = OpenIdApplicationExtensions.ValidateClientSettings(request.ClientType, request.ApplicationType,
            request.ClientSecret, context.RequestServices.GetRequiredService<IStringLocalizer<OpenIdApplicationSettings>>(), isNew, application is not null && await manager.HasClientTypeAsync(application, OpenIddictConstants.ClientTypes.Public, cancellationToken))
            .ToDictionary(error => error.MemberNames.Single() == nameof(OpenIdApplicationSettings.Type) ? "clientType" : "clientSecret",
                error => new[] { error.ErrorMessage });
        return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromServices] IOpenIdScopeManager scopes,
        [FromBody] OpenIdApplicationMutationRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(request) is { } invalid) { return invalid; }
        if (await ValidateReferencesAsync(context, scopes, request) is { } references) { return references; }
        var ct = context.RequestAborted;
        var existing = await manager.FindByClientIdAsync(request.ClientId, ct);
        if (await ValidateClientAsync(context, manager, request, existing, isNew: true, ct) is { } client) { return client; }
        try
        {
            var settings = request.ToSettings();
            if (existing is not null)
            {
                return await MatchesAsync(manager, existing, settings, ct)
                    ? TypedResults.Ok(await OpenIdDiscoveryEndpoints.DescribeAsync(manager, existing, ct))
                    : TypedResults.Problem("This client identifier already has different application settings or credentials.", statusCode: 409);
            }
            await manager.UpdateDescriptorFromSettings(settings, cancellationToken: ct);
            var application = await manager.FindByClientIdAsync(request.ClientId, ct);
            return TypedResults.Created($"{context.Request.PathBase}/api/openid/applications/by-client-id?clientId={Uri.EscapeDataString(request.ClientId)}",
                await OpenIdDiscoveryEndpoints.DescribeAsync(manager, application, ct));
        }
        catch (Exception exception) when (exception is OpenIddictExceptions.ValidationException or UriFormatException)
        {
            return InvalidApplication();
        }
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromServices] IOpenIdScopeManager scopes,
        [FromQuery] string clientId, [FromBody] OpenIdApplicationMutationRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(request) is { } invalid) { return invalid; }
        if (!string.Equals(clientId, request.ClientId, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The body clientId must match the query identifier. Renames are not supported.", statusCode: 400);
        }
        var ct = context.RequestAborted;
        var application = await manager.FindByClientIdForUpdateAsync(clientId, ct);
        if (application is null) { return context.ApiNotFoundProblem(); }
        if (await ValidateReferencesAsync(context, scopes, request) is { } references) { return references; }
        if (await ValidateClientAsync(context, manager, request, application, isNew: false, ct) is { } client) { return client; }
        try
        {
            var settings = request.ToSettings();
            if (!await MatchesAsync(manager, application, settings, ct))
            {
                await manager.UpdateDescriptorFromSettings(settings, application, ct);
            }
            return TypedResults.Ok(await OpenIdDiscoveryEndpoints.DescribeAsync(manager, application, ct));
        }
        catch (Exception exception) when (exception is OpenIddictExceptions.ValidationException or UriFormatException)
        {
            return InvalidApplication();
        }
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromQuery] string clientId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(clientId)) { return TypedResults.Problem("The client identifier is required.", statusCode: 400); }
        var application = await manager.FindByClientIdForUpdateAsync(clientId, context.RequestAborted);
        if (application is not null) { await manager.DeleteAsync(application, context.RequestAborted); }
        return TypedResults.NoContent();
    }

    private static async Task<bool> MatchesAsync(IOpenIdApplicationManager manager, object application,
        OpenIdApplicationSettings settings, CancellationToken cancellationToken)
    {
        var current = new OpenIdApplicationDescriptor();
        await manager.PopulateAsync(current, application, cancellationToken);
        var candidate = await manager.BuildDescriptorFromSettingsAsync(settings, application, cancellationToken);
        return string.Equals(current.ClientId, candidate.ClientId, StringComparison.Ordinal) &&
            string.Equals(current.DisplayName, candidate.DisplayName, StringComparison.Ordinal) &&
            string.Equals(current.ClientType, candidate.ClientType, StringComparison.Ordinal) &&
            string.Equals(current.ApplicationType, candidate.ApplicationType, StringComparison.Ordinal) &&
            string.Equals(current.ConsentType, candidate.ConsentType, StringComparison.Ordinal) &&
            current.Roles.SetEquals(candidate.Roles) && current.Permissions.SetEquals(candidate.Permissions) &&
            current.Requirements.SetEquals(candidate.Requirements) && current.RedirectUris.SetEquals(candidate.RedirectUris) &&
            current.PostLogoutRedirectUris.SetEquals(candidate.PostLogoutRedirectUris) &&
            (string.IsNullOrEmpty(settings.ClientSecret) || await manager.ValidateClientSecretAsync(application, settings.ClientSecret, cancellationToken));
    }

    private static ValidationProblem InvalidApplication() => TypedResults.ValidationProblem(new Dictionary<string, string[]>
    {
        ["application"] = ["The application settings are invalid. Check its client type, credential requirements and absolute redirect URIs without fragments."],
    });
}
