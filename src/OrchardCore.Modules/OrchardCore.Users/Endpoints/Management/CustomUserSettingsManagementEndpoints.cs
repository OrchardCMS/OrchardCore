using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Endpoints.Management;

internal static class CustomUserSettingsManagementEndpoints
{
    public static IEndpointRouteBuilder AddCustomUserSettingsManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        Configure(builder.MapManagementGet("api/users/settings/types", ListAsync), "ApiListCustomUserSettingsTypes", "types", false)
            .WithSummary("Lists authorized custom user settings types.");
        Configure(builder.MapManagementGet("api/users/settings/types/{name}/schema", SchemaAsync), "ApiGetCustomUserSettingsSchema", "schema", false, true)
            .WithSummary("Gets a custom user settings schema.");
        Configure(builder.MapManagementGet("api/users/{userId}/settings/{name}", GetAsync), "ApiGetCustomUserSettings", "show", true, true)
            .WithSummary("Gets custom settings for one user.");
        Configure(builder.MapManagementPut("api/users/{userId}/settings/{name}", UpdateAsync), "ApiUpdateCustomUserSettings", "update", true, true)
            .WithSummary("Updates custom settings on their owning user.")
            .Accepts<JsonObject>("application/json")
            .ProducesValidationProblem();
        return builder;
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder route, string operation, string verb, bool user, bool name = false)
    {
        var metadata = new CliOperationMetadata(["users", "settings"], verb)
        {
            Capability = UserManagementApiEndpointConventions.CapabilityName,
            InputMode = verb == "update" ? CliInputMode.Json : CliInputMode.Options,
        };
        if (user)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("userId", 0));
        }
        if (name)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("name", user ? 1 : 0));
        }

        return route.WithName(operation).WithCliCommand(metadata)
            .Produces<JsonObject>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static Task<IResult> ListAsync(HttpContext context, [FromServices] CustomUserSettingsManagementService service)
        => service.ListAsync(context.User);

    private static Task<IResult> SchemaAsync(string name, HttpContext context, [FromServices] CustomUserSettingsManagementService service)
        => service.SchemaAsync(context.User, name);

    private static Task<IResult> GetAsync(string userId, string name, HttpContext context, [FromServices] CustomUserSettingsManagementService service)
        => service.GetAsync(context.User, userId, name);

    private static Task<IResult> UpdateAsync(string userId, string name, JsonObject input, HttpContext context, [FromServices] CustomUserSettingsManagementService service)
        => context.Request.IsHttps
            ? service.UpdateAsync(context.User, userId, name, input)
            : Task.FromResult(Results.Problem("Custom user settings updates require HTTPS.", statusCode: StatusCodes.Status400BadRequest));
}
