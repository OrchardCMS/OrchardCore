using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings.Services;

namespace OrchardCore.Settings.Endpoints.Api;

internal static class SiteSettingsManagementEndpoints
{
    internal const string Capability = "settings";

    public static IEndpointRouteBuilder AddSiteSettingsManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/settings")
            .WithTags("Settings")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(SettingsPermissions.ManageSettings)))
            .DisableAntiforgery();

        group.MapGet(string.Empty, GetAsync)
            .WithName("ApiGetSiteSettings")
            .WithSummary("Gets the site settings.")
            .WithDescription("Returns the safe core site settings that can be managed remotely.")
            .WithCliCommand(Cli("show"))
            .Produces<SiteSettingsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut(string.Empty, UpdateAsync)
            .WithName("ApiUpdateSiteSettings")
            .WithSummary("Updates the site settings.")
            .WithDescription("Updates the supplied safe core site settings. Omitted properties retain their current values; a JSON request body is required.")
            .WithCliCommand(Cli("update", CliInputMode.Json))
            .Accepts<SiteSettingsUpdateRequest>("application/json")
            .Produces<SiteSettingsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/schema", SchemaAsync)
            .WithName("ApiGetSiteSettingsSchema")
            .WithSummary("Gets the site settings JSON schema.")
            .WithDescription("Returns the safe core update schema and discovery-only schemas explicitly contributed by enabled features.")
            .WithCliCommand(Cli("schema"))
            .Produces<SiteSettingsManagementSchemaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    internal static async Task<IResult> GetAsync(HttpContext httpContext, [FromServices] SiteSettingsManagementService service)
    {
        var result = await service.GetAsync(httpContext.User);
        return result.IsForbidden ? httpContext.ApiForbidProblem() : TypedResults.Ok(result.Value);
    }

    internal static async Task<IResult> UpdateAsync(HttpContext httpContext, [FromServices] SiteSettingsManagementService service, SiteSettingsUpdateRequest request)
    {
        var result = await service.UpdateAsync(httpContext.User, request);
        if (result.IsForbidden)
        {
            return httpContext.ApiForbidProblem();
        }

        if (result.Errors.Count > 0)
        {
            return TypedResults.ValidationProblem(
                result.Errors.ToDictionary(entry => entry.Key, entry => entry.Value),
                detail: string.Join(", ", result.Errors.SelectMany(entry => entry.Value)));
        }

        return TypedResults.Ok(result.Value);
    }

    internal static async Task<IResult> SchemaAsync(HttpContext httpContext, [FromServices] SiteSettingsManagementService service)
    {
        var result = await service.GetSchemaAsync(httpContext.User);
        return result.IsForbidden ? httpContext.ApiForbidProblem() : TypedResults.Ok(result.Value);
    }

    private static CliOperationMetadata Cli(string verb, CliInputMode inputMode = CliInputMode.Options)
        => new(["settings"], verb)
        {
            Capability = Capability,
            InputMode = inputMode,
        };
}
