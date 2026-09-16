using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.RemoteManagement;
using OrchardCore.Queries.Services;

namespace OrchardCore.Queries.Endpoints.Api;

public static class QueryManagementEndpoints
{
    public static IEndpointRouteBuilder AddQueryManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet("api/queries", ListAsync)
            .WithName("ApiListQueries")
            .WithSummary("Lists named queries.")
            .WithDescription("Returns a paged list of stored named queries, optionally filtered by search text or source.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "list")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].source", "Source"),
                    new CliTableColumnMetadata("items[].returnContentItems", "Returns content items"),
                },
            })
            .Produces<PagedQueryDefinitionsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet("api/queries/sources", ListSourcesAsync)
            .WithName("ApiListQuerySources")
            .WithSummary("Lists available query sources.")
            .WithDescription("Returns the query sources that are currently enabled, along with optional source-specific properties schema metadata.")
            .WithCliCommand(new CliOperationMetadata(["queries", "sources"], "list")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].displayName", "Display name"),
                },
            })
            .Produces<PagedQuerySourcesDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost("api/queries", CreateAsync)
            .WithName("ApiCreateQuery")
            .WithSummary("Creates a named query.")
            .WithDescription("Creates and stores a named query definition using the requested source and properties bag.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "create")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
            })
            .Accepts<QueryDefinitionDto>("application/json")
            .Produces<QueryDefinitionDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost("api/queries/validate", ValidateAsync)
            .WithName("ApiValidateQuery")
            .WithSummary("Validates a named query definition.")
            .WithDescription("Validates a named query definition without persisting it, including any source-specific checks contributed by the active query source.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "validate")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
            })
            .Accepts<QueryDefinitionDto>("application/json")
            .Produces<QueryValidationResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet("api/queries/named/{name}", GetAsync)
            .WithName("ApiGetQuery")
            .WithSummary("Shows one named query.")
            .WithDescription("Returns a stored named query definition including its source, properties bag, and optional result and parameter schema metadata.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "show")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                Arguments =
                {
                    new CliArgumentMetadata("name", 0),
                },
            })
            .Produces<QueryDefinitionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPut("api/queries/named/{name}", UpdateAsync)
            .WithName("ApiUpdateQuery")
            .WithSummary("Updates a named query.")
            .WithDescription("Updates a stored named query definition. The request body name must match the route name.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "update")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
                Arguments =
                {
                    new CliArgumentMetadata("name", 0),
                },
            })
            .Accepts<QueryDefinitionDto>("application/json")
            .Produces<QueryDefinitionDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementDelete("api/queries/named/{name}", DeleteAsync)
            .WithName("ApiDeleteQuery")
            .WithSummary("Deletes a named query.")
            .WithDescription("Deletes a stored named query definition.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "delete")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                RequiresConfirmation = true,
                Arguments =
                {
                    new CliArgumentMetadata("name", 0),
                },
            })
            .Produces<QueryDefinitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost("api/queries/named/{name}/execute", ExecuteAsync)
            .WithName("ApiExecuteQuery")
            .WithSummary("Executes a named query.")
            .WithDescription("Executes a stored named query using the supplied JSON parameters object and returns the raw query results.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "execute")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Json,
                DefaultJsonBody = "{}",
                Arguments =
                {
                    new CliArgumentMetadata("name", 0),
                },
            })
            .Accepts<JsonObject>(isOptional: true, "application/json")
            .Produces<QueryExecutionResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<IResult> ListAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        [AsParameters] ListQueriesRequest request)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 20;

        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(detail: "Skip must be zero or greater and take must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > 200)
        {
            return TypedResults.Problem(detail: "Take cannot exceed 200.", statusCode: StatusCodes.Status400BadRequest);
        }

        var queries = (await queryManager.ListQueriesAsync(new QueryContext
        {
            Name = request.Search,
            Source = request.Source,
            Sorted = true,
        })).ToArray();

        return TypedResults.Ok(new PagedQueryDefinitionsDto
        {
            Skip = skip,
            Take = take,
            TotalCount = queries.Length,
            Items = queries.Skip(skip).Take(take).Select(ToDefinitionDto).ToArray(),
        });
    }

    private static async Task<IResult> ListSourcesAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IEnumerable<IQuerySource> querySources,
        [FromServices] IEnumerable<IQuerySourceApiDescriptorProvider> descriptorProviders,
        [AsParameters] ListQuerySourcesRequest request)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 20;

        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(detail: "Skip must be zero or greater and take must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > 200)
        {
            return TypedResults.Problem(detail: "Take cannot exceed 200.", statusCode: StatusCodes.Status400BadRequest);
        }

        var descriptors = querySources
            .OrderBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
            .Select(source =>
            {
                var provider = descriptorProviders.FirstOrDefault(candidate => string.Equals(candidate.Source, source.Name, StringComparison.OrdinalIgnoreCase));

                return new QuerySourceDescriptorDto
                {
                    Name = source.Name,
                    DisplayName = provider?.DisplayName ?? source.Name,
                    Description = provider?.Description,
                    PropertiesSchema = provider?.BuildPropertiesSchema(),
                };
            })
            .ToArray();

        return TypedResults.Ok(new PagedQuerySourcesDto
        {
            Skip = skip,
            Take = take,
            TotalCount = descriptors.Length,
            Items = descriptors.Skip(skip).Take(take).ToArray(),
        });
    }

    private static async Task<IResult> GetAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        string name)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        var query = await queryManager.GetQueryAsync(name);

        if (query is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(ToDefinitionDto(query));
    }

    internal static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        [FromServices] IEnumerable<IQuerySource> querySources,
        [FromServices] IEnumerable<IQuerySourceApiDescriptorProvider> descriptorProviders,
        [FromServices] IStringLocalizer<OrchardCore.Queries.Startup> localizer,
        [FromBody] QueryDefinitionDto definition)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        definition ??= new();
        NormalizeDefinition(definition, querySources, initializeProperties: true);
        var validation = await ValidateDefinitionAsync(queryManager, querySources, descriptorProviders, localizer, definition, requireName: true, currentName: definition.Name);
        if (!validation.IsValid)
        {
            return TypedResults.Problem(detail: string.Join(" ", validation.Errors), statusCode: StatusCodes.Status400BadRequest);
        }

        var existing = await queryManager.GetQueryAsync(definition.Name);
        if (existing is not null)
        {
            var existingDefinition = ToDefinitionDto(existing);
            return AreEquivalent(definition, existingDefinition)
                ? TypedResults.Created($"/api/queries/named/{Uri.EscapeDataString(existing.Name)}", existingDefinition)
                : TypedResults.Problem(
                    detail: $"A query named '{definition.Name}' already exists with a different definition.",
                    statusCode: StatusCodes.Status409Conflict);
        }

        var query = await queryManager.NewAsync(definition.Source, ToQueryData(definition));
        await queryManager.SaveAsync(query);

        return TypedResults.Created($"/api/queries/named/{Uri.EscapeDataString(query.Name)}", ToDefinitionDto(query));
    }

    internal static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        [FromServices] IEnumerable<IQuerySource> querySources,
        [FromServices] IEnumerable<IQuerySourceApiDescriptorProvider> descriptorProviders,
        [FromServices] IStringLocalizer<OrchardCore.Queries.Startup> localizer,
        string name,
        [FromBody] QueryDefinitionDto definition)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        definition ??= new();
        NormalizeDefinition(definition, querySources, initializeProperties: false);
        definition.Name ??= name;
        if (!string.Equals(name, definition.Name, StringComparison.Ordinal))
        {
            return TypedResults.Problem(
                detail: "The query name in the request body must match the route name. Query renames require a dedicated rename operation.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var query = await queryManager.GetQueryAsync(name);
        if (query is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        definition.Source ??= query.Source;
        definition.Schema ??= ParseJsonNode(query.Schema);
        definition.ParameterSchema ??= ParseJsonNode(query.ParameterSchema);
        definition.Properties ??= query.Properties?.DeepClone().AsObject() ?? [];

        var validation = await ValidateDefinitionAsync(queryManager, querySources, descriptorProviders, localizer, definition, requireName: true, currentName: name);
        if (!validation.IsValid)
        {
            return TypedResults.Problem(detail: string.Join(" ", validation.Errors), statusCode: StatusCodes.Status400BadRequest);
        }

        await queryManager.UpdateAsync(query, ToQueryData(definition));

        return TypedResults.Ok(ToDefinitionDto(query));
    }

    internal static async Task<IResult> DeleteAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        string name)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        var query = await queryManager.GetQueryAsync(name);
        if (query is null)
        {
            return TypedResults.NoContent();
        }

        await queryManager.DeleteQueryAsync(name);

        return TypedResults.Ok(ToDefinitionDto(query));
    }

    private static async Task<IResult> ValidateAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        [FromServices] IEnumerable<IQuerySource> querySources,
        [FromServices] IEnumerable<IQuerySourceApiDescriptorProvider> descriptorProviders,
        [FromServices] IStringLocalizer<OrchardCore.Queries.Startup> localizer,
        [FromBody] QueryDefinitionDto definition)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.ManageQueries))
        {
            return httpContext.ApiForbidProblem();
        }

        var validation = await ValidateDefinitionAsync(queryManager, querySources, descriptorProviders, localizer, definition, requireName: false);

        return TypedResults.Ok(validation);
    }

    private static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IQueryManager queryManager,
        string name,
        [FromBody] JsonObject parameters = null)
    {
        var query = await queryManager.GetQueryAsync(name);
        if (query is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, QueryPermissions.CreatePermissionForQuery(query.Name)))
        {
            return httpContext.ApiForbidProblem();
        }

        var result = await queryManager.ExecuteQueryAsync(query, ToParameters(parameters));

        return TypedResults.Ok(new QueryExecutionResultDto
        {
            Count = GetCount(result),
            Items = new JsonArray(result.Items.Select(ToJsonNode).ToArray()),
        });
    }

    private static async Task<QueryValidationResultDto> ValidateDefinitionAsync(
        IQueryManager queryManager,
        IEnumerable<IQuerySource> querySources,
        IEnumerable<IQuerySourceApiDescriptorProvider> descriptorProviders,
        IStringLocalizer<OrchardCore.Queries.Startup> localizer,
        QueryDefinitionDto definition,
        bool requireName,
        string currentName = null)
    {
        var result = new QueryValidationResultDto();
        definition ??= new QueryDefinitionDto();

        if (requireName && string.IsNullOrWhiteSpace(definition.Name))
        {
            result.Errors.Add(localizer["The query name is required."].Value);
        }

        if (string.IsNullOrWhiteSpace(definition.Source))
        {
            result.Errors.Add(localizer["The query source is required."].Value);
            return result;
        }

        if (!querySources.Any(source => string.Equals(source.Name, definition.Source, StringComparison.OrdinalIgnoreCase)))
        {
            result.Errors.Add(localizer["The query source '{0}' is not available.", definition.Source].Value);
            return result;
        }

        if (!IsValidSchemaNode(definition.Schema))
        {
            result.Errors.Add(localizer["The result schema must be a JSON object or boolean JSON schema."].Value);
        }

        if (!IsValidSchemaNode(definition.ParameterSchema))
        {
            result.Errors.Add(localizer["The parameter schema must be a JSON object or boolean JSON schema."].Value);
        }

        if (!string.IsNullOrWhiteSpace(definition.Name)
            && !string.Equals(definition.Name, currentName, StringComparison.Ordinal)
            && await queryManager.GetQueryAsync(definition.Name) is not null)
        {
            result.Errors.Add(localizer["A query named '{0}' already exists.", definition.Name].Value);
        }

        var descriptorProvider = descriptorProviders.FirstOrDefault(candidate => string.Equals(candidate.Source, definition.Source, StringComparison.OrdinalIgnoreCase));
        if (descriptorProvider is not null)
        {
            await descriptorProvider.ValidateAsync(definition, result);
        }

        return result;
    }

    private static QueryDefinitionDto ToDefinitionDto(Query query) => new()
    {
        Name = query.Name,
        Source = query.Source,
        Schema = ParseJsonNode(query.Schema),
        ParameterSchema = ParseJsonNode(query.ParameterSchema),
        ReturnContentItems = query.ReturnContentItems,
        Properties = query.Properties?.DeepClone().AsObject() ?? [],
    };

    private static JsonObject ToQueryData(QueryDefinitionDto definition)
    {
        var data = new JsonObject
        {
            [nameof(Query.Name)] = definition.Name,
            [nameof(Query.Source)] = definition.Source,
            [nameof(Query.ReturnContentItems)] = definition.ReturnContentItems,
            [nameof(Query.Properties)] = definition.Properties?.DeepClone() ?? new JsonObject(),
        };

        if (definition.Schema is not null)
        {
            data[nameof(Query.Schema)] = SerializeJson(definition.Schema);
        }

        if (definition.ParameterSchema is not null)
        {
            data[nameof(Query.ParameterSchema)] = SerializeJson(definition.ParameterSchema);
        }

        return data;
    }

    private static Dictionary<string, object> ToParameters(JsonObject parameters)
    {
        if (parameters is null)
        {
            return [];
        }

        return parameters.ToDictionary(property => property.Key, property => (object)property.Value?.Deserialize<object>());
    }

    private static JsonNode ParseJsonNode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return JsonNode.Parse(value);
    }

    private static string SerializeJson(JsonNode node)
        => node?.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static JsonNode ToJsonNode(object item)
    {
        if (item is null)
        {
            return null;
        }

        if (item is JsonNode node)
        {
            return node.DeepClone();
        }

        return JsonSerializer.SerializeToNode(item);
    }

    private static long? GetCount(IQueryResults result)
    {
        var countProperty = result.GetType().GetProperty("Count");
        if (countProperty?.GetValue(result) is null)
        {
            return result.Items?.LongCount() ?? 0;
        }

        return countProperty.GetValue(result) switch
        {
            int count => count,
            long count => count,
            _ => result.Items?.LongCount() ?? 0,
        };
    }

    private static bool IsValidSchemaNode(JsonNode node)
    {
        if (node is null)
        {
            return true;
        }

        if (node is JsonObject)
        {
            return true;
        }

        return node is JsonValue value && value.TryGetValue<bool>(out _);
    }

    private static void NormalizeDefinition(QueryDefinitionDto definition, IEnumerable<IQuerySource> querySources, bool initializeProperties)
    {
        if (definition is null)
        {
            return;
        }

        definition.Name = definition.Name?.Trim();
        definition.Source = definition.Source?.Trim();
        if (initializeProperties)
        {
            definition.Properties ??= [];
        }

        var source = querySources.FirstOrDefault(candidate => string.Equals(candidate.Name, definition.Source, StringComparison.OrdinalIgnoreCase));
        if (source is not null)
        {
            definition.Source = source.Name;
        }
    }

    private static bool AreEquivalent(QueryDefinitionDto left, QueryDefinitionDto right)
        => string.Equals(left.Name, right.Name, StringComparison.Ordinal)
            && string.Equals(left.Source, right.Source, StringComparison.Ordinal)
            && left.ReturnContentItems == right.ReturnContentItems
            && JsonNode.DeepEquals(left.Schema, right.Schema)
            && JsonNode.DeepEquals(left.ParameterSchema, right.ParameterSchema)
            && JsonNode.DeepEquals(left.Properties, right.Properties);
}
