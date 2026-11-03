using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Endpoints.Management;

internal static class TemplateManagementEndpoints
{
    private const string RoutePrefix = "api/templates";
    private const int DefaultTake = 50;
    public static IEndpointRouteBuilder AddTemplateManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        var routes = builder.MapManagementGroup(TemplateManagementEndpointConventions.TagName, Permissions.ManageTemplates, OrchardCore.Liquid.Permissions.ManageLiquidTemplates);

        routes.MapGet(RoutePrefix, ListAsync)
            .WithName("ApiListTemplates")
            .WithSummary("Lists custom templates.")
            .WithDescription("Returns a paged list of stored custom Liquid templates, optionally filtered by name or description.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "list")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].description", "Description"),
                },
            })
            .Produces<TemplateListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        routes.MapGet(RoutePrefix + "/{name}", GetAsync)
            .WithName("ApiGetTemplate")
            .WithSummary("Shows a custom template.")
            .WithDescription("Returns one stored custom Liquid template by its stable, case-insensitive name.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "show")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<TemplateDefinitionDto>(StatusCodes.Status200OK)
            .ProducesETag()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix, CreateAsync)
            .WithName("ApiCreateTemplate")
            .WithSummary("Creates a custom template.")
            .WithDescription("Creates a complete custom Liquid template definition. An identical stable-name retry returns the existing definition; a different definition conflicts.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "create")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
            })
            .Accepts<TemplateDefinitionDto>("application/json")
            .Produces<TemplateDefinitionDto>(StatusCodes.Status201Created)
            .Produces<TemplateDefinitionDto>(StatusCodes.Status200OK)
            .ProducesETag()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        routes.MapPut(RoutePrefix + "/{name}", UpdateAsync)
            .WithName("ApiUpdateTemplate")
            .WithSummary("Updates a custom template.")
            .WithDescription("Replaces a complete stored custom Liquid template definition. The request body name must match the route name.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "update")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Accepts<TemplateDefinitionDto>("application/json")
            .Produces<TemplateDefinitionDto>(StatusCodes.Status200OK)
            .AcceptsIfMatch()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapDelete(RoutePrefix + "/{name}", DeleteAsync)
            .WithName("ApiDeleteTemplate")
            .WithSummary("Deletes a custom template.")
            .WithDescription("Deletes a stored custom Liquid template. A missing template returns 404.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "delete")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                RequiresConfirmation = true,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<TemplateDefinitionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> ListAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] TemplatesManager templatesManager,
        [AsParameters] TemplateListRequest request)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTemplates) ||
            !await authorizationService.AuthorizeAsync(httpContext.User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return httpContext.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? DefaultTake;
        if (httpContext.ApiPagingProblem(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        var document = await templatesManager.GetTemplatesDocumentAsync();
        var templates = document.Templates
            .Where(entry => string.IsNullOrWhiteSpace(request.Search)
                || entry.Key.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || (entry.Value.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(ToDefinition)
            .ToArray();

        return TypedResults.Ok(new TemplateListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = templates.Length,
            Items = templates.Skip(skip).Take(take).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] TemplatesManager templatesManager,
        string name)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTemplates) ||
            !await authorizationService.AuthorizeAsync(httpContext.User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return httpContext.ApiForbidProblem();
        }

        var document = await templatesManager.GetTemplatesDocumentAsync();
        return TryGetDefinition(document, name, out var definition)
            ? TypedResults.Ok(httpContext.WithApiETag(definition))
            : httpContext.ApiNotFoundProblem();
    }

    internal static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] TemplatesManager templatesManager,
        [FromBody] TemplateDefinitionDto definition)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTemplates) ||
            !await authorizationService.AuthorizeAsync(httpContext.User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return httpContext.ApiForbidProblem();
        }

        if (ValidateDefinition(definition) is { } validationError)
        {
            return validationError;
        }

        var name = definition.Name;
        var result = await templatesManager.CreateTemplateAsync(name, ToTemplate(definition));
        if (result.Status == TemplateCreateStatus.Conflict)
        {
            return TypedResults.Problem(
                detail: $"A template named '{name}' already exists with a different definition.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var created = ToDefinition(result.Name, result.Template);
        httpContext.SetApiETag(created);

        return result.Status == TemplateCreateStatus.Existing
            ? TypedResults.Ok(created)
            : TypedResults.Created(GetLocation(httpContext, result.Name), created);
    }

    internal static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] TemplatesManager templatesManager,
        string name,
        [FromBody] TemplateDefinitionDto definition)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTemplates) ||
            !await authorizationService.AuthorizeAsync(httpContext.User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return httpContext.ApiForbidProblem();
        }

        if (ValidateDefinition(definition) is { } validationError)
        {
            return validationError;
        }

        if (!string.Equals(name, definition.Name, StringComparison.Ordinal))
        {
            return TypedResults.Problem(
                detail: "The template name in the request body must match the route name. Template renames require creating the new name and deleting the old name.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (TryGetDefinition(await templatesManager.GetTemplatesDocumentAsync(), name, out var current) &&
            httpContext.ApiIfMatchProblem(current) is { } preconditionFailed)
        {
            return preconditionFailed;
        }

        var result = await templatesManager.ReplaceTemplateAsync(name, ToTemplate(definition));
        if (result is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(httpContext.WithApiETag(ToDefinition(result.Name, result.Template)));
    }

    internal static async Task<IResult> DeleteAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] TemplatesManager templatesManager,
        string name)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTemplates) ||
            !await authorizationService.AuthorizeAsync(httpContext.User, OrchardCore.Liquid.Permissions.ManageLiquidTemplates))
        {
            return httpContext.ApiForbidProblem();
        }

        var result = await templatesManager.RemoveTemplateIfExistsAsync(name);
        if (result is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(ToDefinition(result.Name, result.Template));
    }

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidateDefinition(TemplateDefinitionDto definition)
    {
        if (definition is null)
        {
            return TypedResults.Problem(
                detail: "A template definition is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            return TypedResults.Problem(
                detail: "The template name is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(definition.Content))
        {
            return TypedResults.Problem(
                detail: "The template content is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }

    private static bool TryGetDefinition(TemplatesDocument document, string name, out TemplateDefinitionDto definition)
    {
        var entry = document.Templates.FirstOrDefault(entry =>
            string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));
        if (entry.Key is null)
        {
            definition = null;
            return false;
        }

        definition = ToDefinition(entry);
        return true;
    }

    private static Template ToTemplate(TemplateDefinitionDto definition) =>
        new()
        {
            Content = definition.Content,
            Description = definition.Description,
        };

    private static TemplateDefinitionDto ToDefinition(KeyValuePair<string, Template> entry) =>
        ToDefinition(entry.Key, entry.Value);

    private static TemplateDefinitionDto ToDefinition(string name, Template template) =>
        new()
        {
            Name = name,
            Content = template.Content,
            Description = template.Description,
        };

    private static string GetLocation(HttpContext httpContext, string name) =>
        $"{httpContext.Request.PathBase}/{RoutePrefix}/{Uri.EscapeDataString(name)}";
}

internal static class TemplateManagementEndpointConventions
{
    public const string CapabilityName = "templates";
    public const string TagName = "Templates";
}
