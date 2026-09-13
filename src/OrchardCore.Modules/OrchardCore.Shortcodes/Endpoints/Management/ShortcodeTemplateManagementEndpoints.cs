using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Endpoints.Management;

internal static class ShortcodeTemplateManagementEndpoints
{
    private const string RoutePrefix = "api/shortcode-templates";
    internal const string CapabilityName = "shortcode-templates";

    public static void AddShortcodeTemplateManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(RoutePrefix, ListAsync), "ApiListShortcodeTemplates", "Lists stored shortcode templates.", "list")
            .Produces<ShortcodeTemplateListResponse>();
        Configure(routes.MapGet(RoutePrefix + "/by-name", GetAsync), "ApiGetShortcodeTemplate", "Shows a stored shortcode template.", "show", argument: true)
            .Produces<ShortcodeTemplateDefinition>().ProducesProblem(404);
        Configure(routes.MapPost(RoutePrefix + "/validate", ValidateAsync), "ApiValidateShortcodeTemplate", "Validates a shortcode name and Liquid template without rendering or saving.", "validate", input: true)
            .Accepts<ShortcodeTemplateDefinition>("application/json").Produces<ShortcodeTemplateValidationResponse>();
        Configure(routes.MapPost(RoutePrefix, CreateAsync), "ApiCreateShortcodeTemplate", "Creates a shortcode template; equivalent stable-name retries return the stored template.", "create", input: true)
            .Accepts<ShortcodeTemplateDefinition>("application/json").Produces<ShortcodeTemplateDefinition>(201).ProducesProblem(409);
        Configure(routes.MapPut(RoutePrefix + "/by-name", UpdateAsync), "ApiUpdateShortcodeTemplate", "Replaces a complete shortcode template. The name cannot change.", "update", argument: true, input: true)
            .Accepts<ShortcodeTemplateDefinition>("application/json").Produces<ShortcodeTemplateDefinition>().ProducesProblem(404);
        Configure(routes.MapDelete(RoutePrefix + "/by-name", DeleteAsync), "ApiDeleteShortcodeTemplate", "Deletes a shortcode template; missing templates are a successful no-op.", "delete", argument: true, confirmation: true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string summary,
        string verb, bool argument = false, bool input = false, bool confirmation = false)
    {
        var metadata = new CliOperationMetadata(["shortcodes", "templates"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = confirmation,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("name", 0));
        }
        return builder.WithName(operationId).WithTags("Shortcode Templates").WithSummary(summary)
            .WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(ShortcodesPermissions.ManageShortcodeTemplates)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [AsParameters] ShortcodeTemplateListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200)
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var document = await manager.GetShortcodeTemplatesDocumentAsync();
        var matches = document.ShortcodeTemplates.Where(entry => string.IsNullOrWhiteSpace(request.Search)
            || entry.Key.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
            || (entry.Value.Hint?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToArray();
        return TypedResults.Ok(new ShortcodeTemplateListResponse
        {
            Skip = skip, Take = take, TotalCount = matches.Length,
            Items = matches.Skip(skip).Take(take).Select(entry => Describe(entry.Key, entry.Value)).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var document = await manager.GetShortcodeTemplatesDocumentAsync();
        var entry = document.ShortcodeTemplates.FirstOrDefault(entry => string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));
        return entry.Key is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(entry.Key, entry.Value));
    }

    internal static async Task<IResult> ValidateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [FromBody] ShortcodeTemplateDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var errors = manager.Validate(definition?.Name, ToTemplate(definition));
        return TypedResults.Ok(new ShortcodeTemplateValidationResponse { IsValid = errors.Count == 0, Errors = errors });
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [FromBody] ShortcodeTemplateDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var result = await manager.SaveAsync(definition?.Name, ToTemplate(definition));
        if (result.Status == ShortcodeTemplateMutationStatus.Invalid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        if (result.Status == ShortcodeTemplateMutationStatus.Conflict)
        {
            return TypedResults.Problem("A shortcode template with this name already exists with a different definition.", statusCode: 409);
        }
        return TypedResults.Created($"{context.Request.PathBase}/{RoutePrefix}/by-name?name={Uri.EscapeDataString(result.Name)}", Describe(result.Name, result.Template));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [FromQuery] string name, [FromBody] ShortcodeTemplateDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        if (definition is null || !string.Equals(name, definition.Name, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The body name must match the name query parameter. Renames are not supported.", statusCode: 400);
        }
        var result = await manager.SaveAsync(name, ToTemplate(definition), name);
        if (result.Status == ShortcodeTemplateMutationStatus.Invalid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        return result.Status == ShortcodeTemplateMutationStatus.NotFound
            ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(result.Name, result.Template));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ShortcodeTemplatesManager manager, [FromQuery] string name)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        await manager.RemoveIfExistsAsync(name);
        return TypedResults.NoContent();
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, ShortcodesPermissions.ManageShortcodeTemplates);

    private static ShortcodeTemplate ToTemplate(ShortcodeTemplateDefinition definition) => definition is null ? null : new()
    {
        Content = definition.Content, Hint = definition.Hint, Usage = definition.Usage,
        DefaultValue = definition.DefaultValue, Categories = definition.Categories,
    };

    private static ShortcodeTemplateDefinition Describe(string name, ShortcodeTemplate template) => new()
    {
        Name = name, Content = template.Content, Hint = template.Hint, Usage = template.Usage,
        DefaultValue = template.DefaultValue, Categories = template.Categories ?? [],
    };
}
