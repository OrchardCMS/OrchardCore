using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.CustomSettings.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.CustomSettings.Endpoints;

internal static class CustomSettingsManagementEndpoints
{
    public static IEndpointRouteBuilder AddCustomSettingsManagementEndpoints(
        this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/custom-settings")
            .WithTags("Custom Settings")
            .DisableAntiforgery()
            .RequireAuthorization(static policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(
                    RemoteManagementPermissions.AccessRemoteManagement))
                .RequireAuthenticatedUser());

        group.MapGet(string.Empty, ListAsync)
            .WithName("ApiListCustomSettings")
            .WithSummary("Lists custom settings.")
            .WithDescription("Returns the CustomSettings content type definitions the current user is authorized to manage.")
            .WithCliCommand(new CliOperationMetadata(["custom-settings"], "list")
            {
                Capability = CustomSettingsRemoteManagementCapabilityProvider.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].displayName", "Display Name"),
                    new CliTableColumnMetadata("items[].description", "Description"),
                },
            })
            .Produces<CustomSettingsListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{name}", GetAsync)
            .WithName("ApiGetCustomSettings")
            .WithSummary("Gets custom settings.")
            .WithDescription("Returns one safe custom settings content envelope.")
            .WithCliCommand(Command("show"))
            .Produces<JsonObject>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{name}", UpdateAsync)
            .WithName("ApiUpdateCustomSettings")
            .WithSummary("Updates custom settings.")
            .WithDescription("Merges a safe JSON payload into one custom settings resource, validates it, and updates the site settings document.")
            .WithCliCommand(Command("update", CliInputMode.Json))
            .Accepts<JsonObject>("application/json")
            .Produces<JsonObject>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{name}/schema", SchemaAsync)
            .WithName("ApiGetCustomSettingsSchema")
            .WithSummary("Gets a custom settings JSON schema.")
            .WithDescription("Returns the sanitized dynamic JSON schema for one custom settings resource.")
            .WithCliCommand(Command("schema"))
            .Produces<JsonObject>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        [FromServices] CustomSettingsManagementService service,
        string search = null,
        int skip = 0,
        int take = 50)
    {
        var response = await service.ListAsync(httpContext.User, search, skip, take);
        return Results.Json(response, service.SerializerOptions);
    }

    private static async Task<IResult> GetAsync(
        string name,
        HttpContext httpContext,
        [FromServices] CustomSettingsManagementService service)
        => ToResult(await service.GetAsync(httpContext.User, name), httpContext, service);

    private static async Task<IResult> UpdateAsync(
        string name,
        JsonObject input,
        HttpContext httpContext,
        [FromServices] CustomSettingsManagementService service)
        => ToResult(await service.UpdateAsync(httpContext.User, name, input), httpContext, service);

    private static async Task<IResult> SchemaAsync(
        string name,
        HttpContext httpContext,
        [FromServices] CustomSettingsManagementService service)
        => ToResult(await service.GetSchemaAsync(httpContext.User, name), httpContext, service);

    private static IResult ToResult(
        CustomSettingsManagementResult<JsonObject> result,
        HttpContext httpContext,
        CustomSettingsManagementService service)
        => result.Status switch
        {
            CustomSettingsManagementStatus.Success =>
                Results.Json(result.Value, service.SerializerOptions),
            CustomSettingsManagementStatus.NotFound =>
                httpContext.ApiNotFoundProblem(),
            CustomSettingsManagementStatus.Forbidden =>
                httpContext.ApiForbidProblem(),
            CustomSettingsManagementStatus.ValidationFailed =>
                TypedResults.ValidationProblem(
                    result.Errors,
                    detail: "The custom settings payload is invalid."),
            _ => throw new InvalidOperationException("Unknown custom settings management result."),
        };

    private static CliOperationMetadata Command(
        string verb,
        CliInputMode inputMode = CliInputMode.Options)
        => new(["custom-settings"], verb)
        {
            Capability = CustomSettingsRemoteManagementCapabilityProvider.CapabilityName,
            InputMode = inputMode,
            Arguments =
            {
                new CliArgumentMetadata("name", 0),
            },
        };
}
