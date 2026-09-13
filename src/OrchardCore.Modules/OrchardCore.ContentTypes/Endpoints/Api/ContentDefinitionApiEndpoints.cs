using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Contents;
using OrchardCore.ContentTypes.Models;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes;

internal static class ContentDefinitionApiEndpoints
{
    internal const string Capability = "content-definitions";
    private const int DefaultTake = 50;
    private const int MaximumTake = 200;

    public static IEndpointRouteBuilder AddContentDefinitionApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/content-definition")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .DisableAntiforgery();

        group.MapGet("/types", ListTypesAsync)
            .WithName("ApiListContentTypeDefinitions")
            .WithTags("Content Definitions")
            .WithSummary("Lists content type definitions.")
            .WithDescription("Returns all content type definitions that the current user can manage.")
            .WithCliCommand(Cli(["content", "types"], "list", tableColumns:
            [
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].displayName", "Display Name"),
            ]))
            .Produces<ContentDefinitionListResponse<ContentTypeDefinitionDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/types/{name}", GetTypeAsync)
            .WithName("ApiGetContentTypeDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Gets a content type definition.")
            .WithDescription("Returns a content type definition, including its attached parts and settings.")
            .WithCliCommand(Cli(["content", "types"], "show", arguments: [new CliArgumentMetadata("name", 0)]))
            .Produces<ContentTypeDefinitionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/types", CreateTypeAsync)
            .WithName("ApiCreateContentTypeDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Creates a content type definition.")
            .WithDescription("Creates a content type definition and optionally configures its attached parts.")
            .WithCliCommand(Cli(["content", "types"], "create", inputMode: CliInputMode.Json))
            .Produces<ContentTypeDefinitionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/types/{name}", UpdateTypeAsync)
            .WithName("ApiUpdateContentTypeDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Updates a content type definition.")
            .WithDescription("Replaces a content type definition, including its attached parts and settings.")
            .WithCliCommand(Cli(["content", "types"], "update", arguments: [new CliArgumentMetadata("name", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentTypeDefinitionDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/types/{name}", DeleteTypeAsync)
            .WithName("ApiDeleteContentTypeDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Deletes a content type definition.")
            .WithDescription("Deletes a content type definition and its type-scoped metadata.")
            .WithCliCommand(Cli(["content", "types"], "delete", arguments: [new CliArgumentMetadata("name", 0)], requiresConfirmation: true))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/part-types", ListPartTypesAsync)
            .WithName("ApiListContentPartTypes")
            .WithTags("Content Definitions")
            .WithSummary("Lists available content part types.")
            .WithDescription("Returns the registered content part types together with their basic JSON schemas.")
            .WithCliCommand(Cli(["content", "part-types"], "list", tableColumns:
            [
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].attachable", "Attachable"),
                new CliTableColumnMetadata("items[].reusable", "Reusable"),
            ]))
            .Produces<ContentDefinitionListResponse<ContentDefinitionPartTypeDescriptor>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/field-types", ListFieldTypesAsync)
            .WithName("ApiListContentFieldTypes")
            .WithTags("Content Definitions")
            .WithSummary("Lists available content field types.")
            .WithDescription("Returns the registered content field types together with their basic JSON schemas.")
            .WithCliCommand(Cli(["content", "field-types"], "list", tableColumns: [new CliTableColumnMetadata("items[].name", "Name")]))
            .Produces<ContentDefinitionListResponse<ContentDefinitionFieldTypeDescriptor>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/settings", ListSettingsSchemasAsync)
            .WithName("ApiListContentDefinitionSettingsSchemas")
            .WithTags("Content Definitions")
            .WithSummary("Lists content definition settings contracts.")
            .WithDescription("Returns module-specific settings contracts contributed by enabled features.")
            .WithCliCommand(Cli(["content", "settings"], "list", tableColumns:
            [
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].scope", "Scope"),
                new CliTableColumnMetadata("items[].appliesTo", "Applies To"),
            ]))
            .Produces<ContentDefinitionListResponse<ContentDefinitionSettingsSchemaDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/settings/{name}", GetSettingsSchemaAsync)
            .WithName("ApiGetContentDefinitionSettingsSchema")
            .WithTags("Content Definitions")
            .WithSummary("Gets a content definition settings contract.")
            .WithDescription("Returns one module-specific settings contract and its JSON Schema.")
            .WithCliCommand(Cli(["content", "settings"], "show", arguments: [new CliArgumentMetadata("name", 0)]))
            .Produces<ContentDefinitionSettingsSchemaDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/parts", ListPartsAsync)
            .WithName("ApiListContentPartDefinitions")
            .WithTags("Content Definitions")
            .WithSummary("Lists content part definitions.")
            .WithDescription("Returns all content part definitions that the current user can manage.")
            .WithCliCommand(Cli(["content", "parts"], "list", tableColumns: [new CliTableColumnMetadata("items[].name", "Name")]))
            .Produces<ContentDefinitionListResponse<ContentPartDefinitionDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/parts/{name}", GetPartAsync)
            .WithName("ApiGetContentPartDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Gets a content part definition.")
            .WithDescription("Returns a content part definition, including its fields and settings.")
            .WithCliCommand(Cli(["content", "parts"], "show", arguments: [new CliArgumentMetadata("name", 0)]))
            .Produces<ContentPartDefinitionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/parts", CreatePartAsync)
            .WithName("ApiCreateContentPartDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Creates a content part definition.")
            .WithDescription("Creates a content part definition and optionally configures its fields.")
            .WithCliCommand(Cli(["content", "parts"], "create", inputMode: CliInputMode.Json))
            .Produces<ContentPartDefinitionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/parts/{name}", UpdatePartAsync)
            .WithName("ApiUpdateContentPartDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Updates a content part definition.")
            .WithDescription("Replaces a content part definition, including its fields and settings.")
            .WithCliCommand(Cli(["content", "parts"], "update", arguments: [new CliArgumentMetadata("name", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentPartDefinitionDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/parts/{name}", DeletePartAsync)
            .WithName("ApiDeleteContentPartDefinition")
            .WithTags("Content Definitions")
            .WithSummary("Deletes a content part definition.")
            .WithDescription("Deletes a content part definition.")
            .WithCliCommand(Cli(["content", "parts"], "delete", arguments: [new CliArgumentMetadata("name", 0)], requiresConfirmation: true))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/parts/{partName}/fields", ListFieldsAsync)
            .WithName("ApiListContentPartFields")
            .WithTags("Content Definitions")
            .WithSummary("Lists a content part's fields.")
            .WithDescription("Returns the field definitions attached to a content part.")
            .WithCliCommand(Cli(["content", "fields"], "list", arguments: [new CliArgumentMetadata("partName", 0)], tableColumns:
            [
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].fieldName", "Field Type"),
            ]))
            .Produces<ContentDefinitionListResponse<ContentPartFieldDefinitionDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/parts/{partName}/fields/{fieldName}", GetFieldAsync)
            .WithName("ApiGetContentPartField")
            .WithTags("Content Definitions")
            .WithSummary("Gets a content part field definition.")
            .WithDescription("Returns a field definition attached to a content part.")
            .WithCliCommand(Cli(["content", "fields"], "show", arguments: [new CliArgumentMetadata("partName", 0), new CliArgumentMetadata("fieldName", 1)]))
            .Produces<ContentPartFieldDefinitionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/parts/{partName}/fields", CreateFieldAsync)
            .WithName("ApiCreateContentPartField")
            .WithTags("Content Definitions")
            .WithSummary("Creates a content part field definition.")
            .WithDescription("Adds a field definition to a content part.")
            .WithCliCommand(Cli(["content", "fields"], "create", arguments: [new CliArgumentMetadata("partName", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentPartFieldDefinitionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/parts/{partName}/fields/{fieldName}", UpdateFieldAsync)
            .WithName("ApiUpdateContentPartField")
            .WithTags("Content Definitions")
            .WithSummary("Updates a content part field definition.")
            .WithDescription("Replaces a field definition attached to a content part.")
            .WithCliCommand(Cli(["content", "fields"], "update", arguments: [new CliArgumentMetadata("partName", 0), new CliArgumentMetadata("fieldName", 1)], inputMode: CliInputMode.Json))
            .Produces<ContentPartFieldDefinitionDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/parts/{partName}/fields/{fieldName}", DeleteFieldAsync)
            .WithName("ApiDeleteContentPartField")
            .WithTags("Content Definitions")
            .WithSummary("Deletes a content part field definition.")
            .WithDescription("Deletes a field definition from a content part.")
            .WithCliCommand(Cli(["content", "fields"], "delete", arguments: [new CliArgumentMetadata("partName", 0), new CliArgumentMetadata("fieldName", 1)], requiresConfirmation: true))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> ListTypesAsync(ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(await service.ListTypesAsync(), skip, take);
    }

    private static async Task<IResult> GetTypeAsync(string name, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var definition = await service.GetTypeAsync(name);
        return definition is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(definition);
    }

    private static async Task<IResult> CreateTypeAsync(ContentTypeDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var definition = await service.CreateTypeAsync(model, modelState);
        return !modelState.IsValid
            ? ValidationProblem(modelState)
            : TypedResults.Created($"/api/content-definition/types/{definition!.Name}", definition);
    }

    private static async Task<IResult> UpdateTypeAsync(string name, ContentTypeDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var definition = await service.UpdateTypeAsync(name, model, modelState);
        if (definition is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid ? ValidationProblem(modelState) : TypedResults.Ok(definition);
    }

    private static async Task<IResult> DeleteTypeAsync(string name, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
        => !await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes)
            ? httpContext.ApiForbidProblem()
            : await DeleteTypeCoreAsync(name, service);

    private static async Task<IResult> DeleteTypeCoreAsync(string name, ContentDefinitionApiService service)
    {
        await service.DeleteTypeAsync(name);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ListPartTypesAsync(ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(await service.ListPartTypesAsync(), skip, take);
    }

    private static async Task<IResult> ListFieldTypesAsync(ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(service.ListFieldTypes(), skip, take);
    }

    private static async Task<IResult> ListSettingsSchemasAsync(ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(service.ListSettingsSchemas(), skip, take);
    }

    private static async Task<IResult> GetSettingsSchemaAsync(string name, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return service.GetSettingsSchema(name) is { } schema
            ? TypedResults.Ok(schema)
            : httpContext.ApiNotFoundProblem();
    }

    private static async Task<IResult> ListPartsAsync(ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(await service.ListPartsAsync(), skip, take);
    }

    private static async Task<IResult> GetPartAsync(string name, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var definition = await service.GetPartAsync(name);
        return definition is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(definition);
    }

    private static async Task<IResult> CreatePartAsync(ContentPartDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var definition = await service.CreatePartAsync(model, modelState);
        return !modelState.IsValid
            ? ValidationProblem(modelState)
            : TypedResults.Created($"/api/content-definition/parts/{definition!.Name}", definition);
    }

    private static async Task<IResult> UpdatePartAsync(string name, ContentPartDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var definition = await service.UpdatePartAsync(name, model, modelState);
        if (definition is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid ? ValidationProblem(modelState) : TypedResults.Ok(definition);
    }

    private static async Task<IResult> DeletePartAsync(string name, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
        => !await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes)
            ? httpContext.ApiForbidProblem()
            : await DeletePartCoreAsync(name, service);

    private static async Task<IResult> DeletePartCoreAsync(string name, ContentDefinitionApiService service)
    {
        await service.DeletePartAsync(name);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ListFieldsAsync(string partName, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var fields = await service.ListFieldsAsync(partName);
        return fields is null ? httpContext.ApiNotFoundProblem() : Page(fields, skip, take);
    }

    private static async Task<IResult> GetFieldAsync(string partName, string fieldName, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.ViewContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var field = await service.GetFieldAsync(partName, fieldName);
        return field is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(field);
    }

    private static async Task<IResult> CreateFieldAsync(string partName, ContentPartFieldDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var field = await service.CreateFieldAsync(partName, model, modelState);
        if (field is null && modelState.IsValid)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid
            ? ValidationProblem(modelState)
            : TypedResults.Created($"/api/content-definition/parts/{partName}/fields/{field!.Name}", field);
    }

    private static async Task<IResult> UpdateFieldAsync(string partName, string fieldName, ContentPartFieldDefinitionDto model, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var field = await service.UpdateFieldAsync(partName, fieldName, model, modelState);
        if (field is null && modelState.IsValid)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid ? ValidationProblem(modelState) : TypedResults.Ok(field);
    }

    private static async Task<IResult> DeleteFieldAsync(string partName, string fieldName, ContentDefinitionApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ContentTypesPermissions.EditContentTypes))
        {
            return httpContext.ApiForbidProblem();
        }

        await service.DeleteFieldAsync(partName, fieldName);
        return TypedResults.NoContent();
    }

    private static CliOperationMetadata Cli(
        string[] commandGroup,
        string verb,
        CliArgumentMetadata[] arguments = null,
        string[] aliases = null,
        CliTableColumnMetadata[] tableColumns = null,
        CliInputMode inputMode = CliInputMode.Options,
        bool requiresConfirmation = false)
    {
        var metadata = new CliOperationMetadata(commandGroup, verb)
        {
            Capability = Capability,
            InputMode = inputMode,
            RequiresConfirmation = requiresConfirmation,
        };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
            {
                metadata.Arguments.Add(argument);
            }
        }

        if (aliases is not null)
        {
            foreach (var alias in aliases)
            {
                metadata.Aliases.Add(alias);
            }
        }

        if (tableColumns is not null)
        {
            foreach (var column in tableColumns)
            {
                metadata.TableColumns.Add(column);
            }
        }

        return metadata;
    }

    private static ValidationProblem ValidationProblem(ModelStateDictionary modelState)
        => TypedResults.ValidationProblem(modelState.ToDictionary(
            entry => entry.Key,
            entry => entry.Value?.Errors.Select(error => error.ErrorMessage).ToArray() ?? []),
            detail: string.Join(", ", modelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))));

    private static IResult Page<T>(IReadOnlyList<T> items, int? requestedSkip, int? requestedTake)
    {
        var skip = requestedSkip ?? 0;
        var take = requestedTake ?? DefaultTake;

        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(
                detail: "Skip must be zero or greater and take must be greater than zero.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > MaximumTake)
        {
            return TypedResults.Problem(
                detail: $"Take cannot exceed {MaximumTake}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(new ContentDefinitionListResponse<T>
        {
            Skip = skip,
            Take = take,
            TotalCount = items.Count,
            Items = items.Skip(skip).Take(take).ToArray(),
        });
    }
}
