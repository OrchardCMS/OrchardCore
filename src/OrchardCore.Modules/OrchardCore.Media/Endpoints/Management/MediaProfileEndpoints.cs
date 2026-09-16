using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Endpoints.Management;

internal static class MediaProfileEndpoints
{
    public static void AddMediaProfileEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/media/profiles", ListAsync), "ApiListMediaProfiles", "list", "Lists named image profiles.")
            .Produces<IReadOnlyList<MediaProfileDefinition>>();
        Configure(routes.MapGet("api/media/profiles/by-name", GetAsync), "ApiGetMediaProfile", "show", "Shows a complete image profile.", "name")
            .Produces<MediaProfileDefinition>();
        Configure(routes.MapPost("api/media/profiles", CreateAsync), "ApiCreateMediaProfile", "create", "Creates a profile; identical name retries return the existing profile.", input: true)
            .Produces<MediaProfileDefinition>();
        Configure(routes.MapPut("api/media/profiles/by-name", UpdateAsync), "ApiUpdateMediaProfile", "update", "Replaces or renames a profile after validating its definition and name conflicts.", "name", true)
            .Produces<MediaProfileDefinition>();
        Configure(routes.MapDelete("api/media/profiles/by-name", DeleteAsync), "ApiDeleteMediaProfile", "delete", "Deletes a named profile; an absent profile is unchanged.", "name")
            .Produces(204);
    }

    internal static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary,
        string argument = null, bool input = false, string[] group = null, Permission permission = null)
    {
        var metadata = new CliOperationMetadata(group ?? ["media", "profiles"], verb)
        {
            Capability = "media", InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = verb is "delete" or "purge",
        };
        if (argument is not null) { metadata.Arguments.Add(new CliArgumentMetadata(argument, 0)); }
        return builder.WithName(name).WithTags("Media Administration").WithSummary(summary).WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(
                    new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(permission ?? MediaPermissions.ManageMediaProfiles)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, MediaPermissions.ManageMediaProfiles);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaProfilesManager manager, [FromQuery] int? skip, [FromQuery] int? take)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (skip < 0 || take < 1 || take > 200) { return Invalid("Skip must be nonnegative and take between 1 and 200."); }
        var document = await manager.GetMediaProfilesDocumentAsync();
        return TypedResults.Ok<IReadOnlyList<MediaProfileDefinition>>(document.MediaProfiles.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Skip(skip ?? 0).Take(take ?? 50).Select(pair => Describe(pair.Key, pair.Value)).ToArray());
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaProfilesManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(name)) { return Invalid("A profile name is required."); }
        var document = await manager.GetMediaProfilesDocumentAsync();
        return document.MediaProfiles.TryGetValue(name, out var profile) ? TypedResults.Ok(Describe(name, profile)) : context.ApiNotFoundProblem();
    }

    internal static Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaProfileManagementService service, [FromBody] MediaProfileDefinition input) => SaveAsync(context, authorization, service, input, null);

    internal static Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaProfileManagementService service, [FromQuery] string name, [FromBody] MediaProfileDefinition input) => SaveAsync(context, authorization, service, input, name);

    private static async Task<IResult> SaveAsync(HttpContext context, IAuthorizationService authorization,
        MediaProfileManagementService service, MediaProfileDefinition input, string sourceName)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (input is null || !Enum.TryParse<ResizeMode>(input.Mode, out var mode) || mode.ToString() != input.Mode
            || !Enum.TryParse<Format>(input.Format, out var format) || format.ToString() != input.Format)
        {
            return Invalid("A complete profile definition with named mode and format values is required.");
        }
        var result = await service.SaveAsync(input.Name, new MediaProfile
        {
            Hint = input.Hint, Width = input.Width, Height = input.Height, Mode = mode, Format = format,
            Quality = input.Quality, BackgroundColor = input.BackgroundColor, AutoOrient = input.AutoOrient,
        }, sourceName);
        if (result.Errors.Count > 0) { return TypedResults.ValidationProblem(result.Errors); }
        if (result.NotFound) { return context.ApiNotFoundProblem(); }
        if (result.Conflict) { return TypedResults.Problem("A different profile already uses this name.", statusCode: 409); }
        return TypedResults.Ok(Describe(result.Name, result.Profile));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaProfilesManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(name)) { return Invalid("A profile name is required."); }
        var document = await manager.LoadMediaProfilesDocumentAsync();
        if (document.MediaProfiles.ContainsKey(name)) { await manager.RemoveMediaProfileAsync(name); }
        return TypedResults.NoContent();
    }

    private static MediaProfileDefinition Describe(string name, MediaProfile profile) => new()
    {
        Name = name, Hint = profile.Hint, Width = profile.Width, Height = profile.Height, Mode = profile.Mode.ToString(),
        Format = profile.Format.ToString(), Quality = profile.Quality, BackgroundColor = profile.BackgroundColor, AutoOrient = profile.AutoOrient,
    };
    private static ProblemHttpResult Invalid(string message) => TypedResults.Problem(message, statusCode: 400);
}

/// <summary>A complete named image profile, using named resize modes and formats.</summary>
public sealed class MediaProfileDefinition
{
    /// <summary>Gets or sets the case-insensitive profile name, stored in invariant lowercase.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the optional administrative hint.</summary>
    public string Hint { get; set; }
    /// <summary>Gets or sets the nonnegative width; zero leaves it unspecified.</summary>
    public int Width { get; set; }
    /// <summary>Gets or sets the nonnegative height; zero leaves it unspecified.</summary>
    public int Height { get; set; }
    /// <summary>Gets or sets Undefined, Max, Crop, Pad, BoxPad, Min, or Stretch.</summary>
    public string Mode { get; set; } = "Undefined";
    /// <summary>Gets or sets Undefined, Gif, Jpg, Png, or WebP.</summary>
    public string Format { get; set; } = "Undefined";
    /// <summary>Gets or sets quality from zero to 100; zero uses the engine default.</summary>
    public int Quality { get; set; } = 100;
    /// <summary>Gets or sets a three- or six-digit RGB hex color, optionally prefixed by #.</summary>
    public string BackgroundColor { get; set; }
    /// <summary>Gets or sets whether image orientation is applied.</summary>
    public bool AutoOrient { get; set; } = true;
}
