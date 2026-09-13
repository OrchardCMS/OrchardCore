using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenId.Endpoints.Management;

internal static class OpenIdDiscoveryEndpoints
{
    internal const string CapabilityName = "openid-management";

    public static void AddOpenIdDiscoveryEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/openid/applications", ListApplicationsAsync), "ApiListOpenIdApplications",
            "Lists applications without credentials or private settings.", "applications", "list", OpenIdPermissions.ManageApplications)
            .Produces<OpenIdListResponse<OpenIdApplicationResponse>>();
        Configure(routes.MapGet("api/openid/applications/by-client-id", GetApplicationAsync), "ApiGetOpenIdApplication",
            "Shows an application by its client identifier without credentials or private settings.", "applications", "show", OpenIdPermissions.ManageApplications, "clientId")
            .Produces<OpenIdApplicationResponse>().ProducesProblem(404);
        Configure(routes.MapGet("api/openid/scopes", ListScopesAsync), "ApiListOpenIdScopes",
            "Lists registered scopes and resources.", "scopes", "list", OpenIdPermissions.ManageScopes)
            .Produces<OpenIdListResponse<OpenIdScopeResponse>>();
        Configure(routes.MapGet("api/openid/scopes/by-name", GetScopeAsync), "ApiGetOpenIdScope",
            "Shows a registered scope by name.", "scopes", "show", OpenIdPermissions.ManageScopes, "name")
            .Produces<OpenIdScopeResponse>().ProducesProblem(404);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string id, string summary,
        string resource, string verb, Permission permission, string argument = null)
    {
        var metadata = new CliOperationMetadata(["openid", resource], verb) { Capability = CapabilityName };
        if (argument is not null)
        {
            metadata.Arguments.Add(new CliArgumentMetadata(argument, 0));
        }
        return builder.WithName(id).WithTags("OpenID Management").WithSummary(summary).WithCliCommand(metadata)
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(permission)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization, Permission permission) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, permission);

    private static bool ValidPage(OpenIdListRequest request) =>
        (request.Skip ?? 0) >= 0 && (request.Take ?? 50) is >= 1 and <= 200;

    internal static async Task<IResult> ListApplicationsAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [AsParameters] OpenIdListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization, OpenIdPermissions.ManageApplications))
        {
            return context.ApiForbidProblem();
        }
        if (!ValidPage(request))
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        var cancellationToken = context.RequestAborted;
        var total = await manager.CountAsync(cancellationToken);
        var items = new List<OpenIdApplicationResponse>();
        await foreach (var item in manager.ListAsync(take, skip, cancellationToken))
        {
            items.Add(await DescribeAsync(manager, item, cancellationToken));
        }
        return TypedResults.Ok(new OpenIdListResponse<OpenIdApplicationResponse>
        {
            Skip = skip, Take = take, TotalCount = total, Items = items,
        });
    }

    internal static async Task<IResult> GetApplicationAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdApplicationManager manager, [FromQuery] string clientId)
    {
        if (!await AuthorizedAsync(context, authorization, OpenIdPermissions.ManageApplications))
        {
            return context.ApiForbidProblem();
        }
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return TypedResults.Problem("The identifier must not be empty.", statusCode: 400);
        }
        var item = await manager.FindByClientIdAsync(clientId, context.RequestAborted);
        return item is null ? context.ApiNotFoundProblem() : TypedResults.Ok(await DescribeAsync(manager, item, context.RequestAborted));
    }

    internal static async Task<IResult> ListScopesAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdScopeManager manager, [AsParameters] OpenIdListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization, OpenIdPermissions.ManageScopes))
        {
            return context.ApiForbidProblem();
        }
        if (!ValidPage(request))
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        var cancellationToken = context.RequestAborted;
        var total = await manager.CountAsync(cancellationToken);
        var items = new List<OpenIdScopeResponse>();
        await foreach (var item in manager.ListAsync(take, skip, cancellationToken))
        {
            items.Add(await DescribeAsync(manager, item, cancellationToken));
        }
        return TypedResults.Ok(new OpenIdListResponse<OpenIdScopeResponse>
        {
            Skip = skip, Take = take, TotalCount = total, Items = items,
        });
    }

    internal static async Task<IResult> GetScopeAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOpenIdScopeManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization, OpenIdPermissions.ManageScopes))
        {
            return context.ApiForbidProblem();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return TypedResults.Problem("The identifier must not be empty.", statusCode: 400);
        }
        var item = await manager.FindByNameAsync(name, context.RequestAborted);
        return item is null ? context.ApiNotFoundProblem() : TypedResults.Ok(await DescribeAsync(manager, item, context.RequestAborted));
    }

    internal static async Task<OpenIdApplicationResponse> DescribeAsync(IOpenIdApplicationManager manager, object item, CancellationToken cancellationToken) =>
        new()
        {
            Id = await manager.GetPhysicalIdAsync(item, cancellationToken),
            ClientId = await manager.GetClientIdAsync(item, cancellationToken),
            DisplayName = await manager.GetDisplayNameAsync(item, cancellationToken),
            ClientType = await manager.GetClientTypeAsync(item, cancellationToken),
            ApplicationType = await manager.GetApplicationTypeAsync(item, cancellationToken),
            ConsentType = await manager.GetConsentTypeAsync(item, cancellationToken),
            Roles = await manager.GetRolesAsync(item, cancellationToken),
            Permissions = await manager.GetPermissionsAsync(item, cancellationToken),
            Requirements = await manager.GetRequirementsAsync(item, cancellationToken),
            RedirectUris = await manager.GetRedirectUrisAsync(item, cancellationToken),
            PostLogoutRedirectUris = await manager.GetPostLogoutRedirectUrisAsync(item, cancellationToken),
        };

    internal static async Task<OpenIdScopeResponse> DescribeAsync(IOpenIdScopeManager manager, object item, CancellationToken cancellationToken) =>
        new()
        {
            Id = await manager.GetPhysicalIdAsync(item, cancellationToken),
            Name = await manager.GetNameAsync(item, cancellationToken),
            DisplayName = await manager.GetDisplayNameAsync(item, cancellationToken),
            Description = await manager.GetDescriptionAsync(item, cancellationToken),
            Resources = await manager.GetResourcesAsync(item, cancellationToken),
        };
}
