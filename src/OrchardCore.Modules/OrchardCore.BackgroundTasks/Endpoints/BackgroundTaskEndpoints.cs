using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.BackgroundTasks.Endpoints;

internal static class BackgroundTaskEndpoints
{
    public static void AddBackgroundTaskEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/background-tasks", ListAsync), "ApiListBackgroundTasks", "list", "Lists registered tenant tasks and their effective settings.")
            .Produces<IReadOnlyList<BackgroundTaskResponse>>();
        Configure(routes.MapGet("api/background-tasks/by-name", GetAsync), "ApiGetBackgroundTask", "show", "Shows effective settings for a registered task.", true)
            .Produces<BackgroundTaskResponse>();
        Configure(routes.MapPost("api/background-tasks/validate", ValidateAsync), "ApiValidateBackgroundTask", "validate", "Validates cron and lock settings without saving or executing a task.", input: true)
            .Produces<BackgroundTaskValidationResponse>();
        Configure(routes.MapPut("api/background-tasks/by-name", UpdateAsync), "ApiUpdateBackgroundTask", "update", "Replaces task configuration while preserving its enabled status.", true, true)
            .Produces<BackgroundTaskResponse>();
        Configure(routes.MapPost("api/background-tasks/enable", EnableAsync), "ApiEnableBackgroundTask", "enable", "Enables a registered task after validating its settings.", true)
            .Produces(204);
        Configure(routes.MapPost("api/background-tasks/disable", DisableAsync), "ApiDisableBackgroundTask", "disable", "Disables future scheduling of a registered task; does not cancel running work.", true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary, bool argument = false, bool input = false)
    {
        var metadata = new CliOperationMetadata(["background-tasks"], verb)
        {
            Capability = "background-tasks", InputMode = input ? CliInputMode.Json : CliInputMode.Options,
        };
        if (argument) { metadata.Arguments.Add(new CliArgumentMetadata("name", 0)); }
        return builder.WithName(name).WithTags("Background Tasks").WithSummary(summary).WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(
                    new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(Permissions.ManageBackgroundTasks)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, Permissions.ManageBackgroundTasks);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] BackgroundTaskManagementService tasks, [FromQuery] string search, [FromQuery] bool? enabled,
        [FromQuery] int? skip, [FromQuery] int? take)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (skip < 0 || take < 1 || take > 200 || search?.Length > 1024)
        {
            return TypedResults.Problem("Skip must be nonnegative, take between 1 and 200, and search at most 1024 characters.", statusCode: 400);
        }
        var all = await tasks.ListAsync();
        return TypedResults.Ok<IReadOnlyList<BackgroundTaskResponse>>(all
            .Where(task => enabled is null || task.Enable == enabled)
            .Where(task => string.IsNullOrWhiteSpace(search) || task.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (task.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (task.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(task => task.Name, StringComparer.Ordinal).Skip(skip ?? 0).Take(take ?? 50).Select(Describe).ToArray());
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] BackgroundTaskManagementService tasks, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var settings = await tasks.GetAsync(name);
        return settings is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(settings));
    }

    internal static async Task<IResult> ValidateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromBody] BackgroundTaskConfiguration input)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var errors = BackgroundTaskManagementService.Validate(input);
        return TypedResults.Ok(new BackgroundTaskValidationResponse { IsValid = errors.Count == 0, Errors = errors });
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] BackgroundTaskManagementService tasks, [FromQuery] string name, [FromBody] BackgroundTaskConfiguration input)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var result = await tasks.UpdateAsync(name, input);
        if (result.Errors.Count > 0) { return TypedResults.ValidationProblem(result.Errors); }
        return !result.Found ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(result.Settings));
    }

    internal static Task<IResult> EnableAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] BackgroundTaskManagementService tasks, [FromQuery] string name) => StatusAsync(context, authorization, tasks, name, true);

    internal static Task<IResult> DisableAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] BackgroundTaskManagementService tasks, [FromQuery] string name) => StatusAsync(context, authorization, tasks, name, false);

    private static async Task<IResult> StatusAsync(HttpContext context, IAuthorizationService authorization,
        BackgroundTaskManagementService tasks, string name, bool enabled)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var result = await tasks.SetStatusAsync(name, enabled);
        if (result.Errors.Count > 0) { return TypedResults.ValidationProblem(result.Errors); }
        return !result.Found ? context.ApiNotFoundProblem() : TypedResults.NoContent();
    }

    private static BackgroundTaskResponse Describe(BackgroundTaskSettings settings) => new()
    {
        Name = settings.Name, Title = settings.Title, Enabled = settings.Enable,
        Configuration = BackgroundTaskManagementService.Configuration(settings),
    };
}

/// <summary>Effective settings for one registered tenant task, without invented execution or cancellation state.</summary>
public sealed class BackgroundTaskResponse
{
    /// <summary>Gets the registered task name used for subsequent commands.</summary>
    public string Name { get; init; }
    /// <summary>Gets the display title supplied by the task.</summary>
    public string Title { get; init; }
    /// <summary>Gets whether future scheduling is enabled.</summary>
    public bool Enabled { get; init; }
    /// <summary>Gets the complete writable task configuration.</summary>
    public BackgroundTaskConfiguration Configuration { get; init; }
}

/// <summary>Reports settings validation without a write or task execution.</summary>
public sealed class BackgroundTaskValidationResponse
{
    /// <summary>Gets whether the cron and lock configuration are valid.</summary>
    public bool IsValid { get; init; }
    /// <summary>Gets errors keyed by writable property name.</summary>
    public IDictionary<string, string[]> Errors { get; init; }
}
