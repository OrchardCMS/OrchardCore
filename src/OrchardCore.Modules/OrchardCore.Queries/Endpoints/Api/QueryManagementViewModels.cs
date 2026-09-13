using System.Text.Json.Nodes;

namespace OrchardCore.Queries.Endpoints.Api;

/// <summary>
/// Describes the query string used to browse stored queries.
/// </summary>
public sealed class ListQueriesRequest
{
    public string Search { get; set; }

    public string Source { get; set; }

    public int? Skip { get; set; }

    public int? Take { get; set; }
}

/// <summary>
/// Describes the query string used to browse available query sources.
/// </summary>
public sealed class ListQuerySourcesRequest
{
    /// <summary>
    /// Gets or sets the number of query sources to skip.
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of query sources to return.
    /// </summary>
    public int? Take { get; set; }
}

/// <summary>
/// Describes a named query definition.
/// </summary>
public sealed class QueryDefinitionDto
{
    public string Name { get; set; }

    public string Source { get; set; }

    public JsonNode Schema { get; set; }

    public JsonNode ParameterSchema { get; set; }

    public bool ReturnContentItems { get; set; }

    public JsonObject Properties { get; set; }
}

/// <summary>
/// Describes a paged list of named queries.
/// </summary>
public sealed class PagedQueryDefinitionsDto
{
    public int Skip { get; set; }

    public int Take { get; set; }

    public int TotalCount { get; set; }

    public IReadOnlyList<QueryDefinitionDto> Items { get; set; } = [];
}

/// <summary>
/// Describes a query source that can create or execute named queries.
/// </summary>
public sealed class QuerySourceDescriptorDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public JsonNode PropertiesSchema { get; set; }
}

/// <summary>
/// Describes a paged list of query sources.
/// </summary>
public sealed class PagedQuerySourcesDto
{
    /// <summary>
    /// Gets or sets the number of query sources skipped.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of query sources requested.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Gets or sets the total number of query sources.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the query sources in the current page.
    /// </summary>
    public IReadOnlyList<QuerySourceDescriptorDto> Items { get; set; } = [];
}

/// <summary>
/// Describes the result of validating a named query definition.
/// </summary>
public sealed class QueryValidationResultDto
{
    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];
}

/// <summary>
/// Describes the result of executing a named query.
/// </summary>
public sealed class QueryExecutionResultDto
{
    public long? Count { get; set; }

    public JsonArray Items { get; set; } = [];
}
