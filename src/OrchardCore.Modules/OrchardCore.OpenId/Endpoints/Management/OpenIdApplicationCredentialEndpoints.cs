using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.OpenId.Endpoints.Management;

internal static class OpenIdApplicationCredentialEndpoints
{
    public static void AddOpenIdApplicationCredentialEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapPost("api/openid/applications/credentials:rotate", RotateAsync), "ApiRotateOpenIdApplicationCredential", "rotate",
            "Replaces the shared secret immediately and returns its replacement once. Repeating rotation replaces it again.", secretResponse: true)
            .Produces<RemoteManagementClientCredentials>().ProducesProblem(404);
        Configure(routes.MapPost("api/openid/applications/credentials:revoke", RevokeAsync), "ApiRevokeOpenIdApplicationCredential", "revoke",
            "Replaces the shared secret with an undisclosed value. Issued tokens and other authentication methods are unaffected.")
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string id, string verb, string summary, bool secretResponse = false) =>
        builder.WithName(id).WithTags("OpenID Management").WithSummary(summary)
            .WithCliCommand(new CliOperationMetadata(["openid", "applications", "credentials"], verb)
            {
                Capability = OpenIdDiscoveryEndpoints.CapabilityName,
                Arguments = { new CliArgumentMetadata("clientId", 0) },
                RequiresConfirmation = true,
                SecretResponse = secretResponse,
            })
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(OpenIdPermissions.ManageApplications)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);

    internal static Task<IResult> RotateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromQuery] string clientId) =>
        ChangeAsync(context, authorization, manager, clientId, returnSecret: true);

    internal static Task<IResult> RevokeAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromQuery] string clientId) =>
        ChangeAsync(context, authorization, manager, clientId, returnSecret: false);

    private static async Task<IResult> ChangeAsync(HttpContext context, IAuthorizationService authorization,
        IOpenIdApplicationManager manager, string clientId, bool returnSecret)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) ||
            !await authorization.AuthorizeAsync(context.User, OpenIdPermissions.ManageApplications))
        {
            return context.ApiForbidProblem();
        }
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return TypedResults.Problem("The client identifier is required.", statusCode: 400);
        }
        var ct = context.RequestAborted;
        var application = await manager.FindByClientIdForUpdateAsync(clientId, ct);
        if (application is null)
        {
            return returnSecret ? context.ApiNotFoundProblem() : TypedResults.NoContent();
        }
        if (!await manager.HasClientTypeAsync(application, OpenIddictConstants.ClientTypes.Confidential, ct))
        {
            return TypedResults.Problem("Only confidential applications have a shared secret to rotate or revoke.", statusCode: 400);
        }
        var secret = RemoteManagementClientCredentials.GenerateSecret();
        try
        {
            await manager.UpdateWithValidationRollbackAsync(application, () => manager.UpdateAsync(application, secret, ct), ct);
        }
        catch (OpenIddictExceptions.ValidationException)
        {
            return TypedResults.Problem("The application rejected the credential update. Its previous settings were restored.", statusCode: 400);
        }
        return returnSecret ? TypedResults.Ok(new RemoteManagementClientCredentials
        {
            ClientId = await manager.GetClientIdAsync(application, ct),
            ClientSecret = secret,
        }) : TypedResults.NoContent();
    }
}
