using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DataSources.Files;
using OrchardCore.Queries;

namespace OrchardCore.DataSources.Queries;

/// <summary>
/// Exposes each saved query as a data set, to users allowed to execute it. A query that returns content items has the
/// common columns of content items; the columns of any other query are inferred from a sample of its results. Query
/// parameters are passed with <see cref="DataSourceQuery.Parameters"/>.
/// </summary>
public sealed class QueriesDataSource : IDataSource
{
    /// <summary>
    /// The technical name of the data source.
    /// </summary>
    public const string SourceName = "Queries";

    private const int SampleSize = 20;

    private readonly IQueryManager _queryManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer S;

    public QueriesDataSource(
        IQueryManager queryManager,
        IAuthorizationService authorizationService,
        IStringLocalizer<QueriesDataSource> localizer)
    {
        _queryManager = queryManager;
        _authorizationService = authorizationService;
        S = localizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Queries"];

    public LocalizedString Description => S["The results of the saved queries."];

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new List<DataSetDescriptor>();

        foreach (var query in await _queryManager.ListQueriesAsync())
        {
            if (await CanExecuteAsync(context.User, query))
            {
                result.Add(Describe(query));
            }
        }

        return result
            .OrderBy(dataSet => dataSet.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = await GetExecutableQueryAsync(dataSet, context.User);

        if (query is null)
        {
            return null;
        }

        IReadOnlyList<DataField> fields;

        if (query.ReturnContentItems)
        {
            fields = ContentFields();
        }
        else
        {
            var results = await _queryManager.ExecuteQueryAsync(query, new Dictionary<string, object>());
            fields = JsonDataRecords.InferFields((results?.Items ?? []).Take(SampleSize).Select(ToJsonObject).Where(item => item is not null));
        }

        return new DataSetSchema
        {
            DataSet = Describe(query),
            Fields = [.. fields],
        };
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var savedQuery = await GetExecutableQueryAsync(query.DataSet, query.Context?.User);

        if (savedQuery is null)
        {
            yield break;
        }

        var parameters = new Dictionary<string, object>(query.Parameters ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase);
        var results = await _queryManager.ExecuteQueryAsync(savedQuery, parameters);
        var items = results?.Items ?? [];

        if (query.MaxRows > 0)
        {
            items = items.Take(query.MaxRows);
        }

        var batchSize = Math.Max(1, query.BatchSize);

        if (savedQuery.ReturnContentItems)
        {
            var fields = ContentFields();

            foreach (var chunk in items.OfType<ContentItem>().Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new DataBatch(fields, chunk.Select(ToContentRow).ToList());
            }

            yield break;
        }

        await foreach (var batch in JsonDataRecords.ToBatchesAsync(ToAsync(items.Select(ToJsonObject).Where(item => item is not null)), batchSize, cancellationToken))
        {
            yield return batch;
        }
    }

    private static async IAsyncEnumerable<JsonObject> ToAsync(IEnumerable<JsonObject> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static JsonObject ToJsonObject(object item)
        => item switch
        {
            null => null,
            JsonObject jsonObject => jsonObject,
            JsonNode node => new JsonObject { ["Value"] = node.DeepClone() },
            _ => JsonSerializer.SerializeToNode(item) is JsonObject converted ? converted : new JsonObject { ["Value"] = JsonValue.Create(DataValues.ToText(item)) },
        };

    private async Task<Query> GetExecutableQueryAsync(string name, ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var query = await _queryManager.GetQueryAsync(name);

        return query is not null && await CanExecuteAsync(user, query) ? query : null;
    }

    private async Task<bool> CanExecuteAsync(ClaimsPrincipal user, Query query)
        => user is not null && await _authorizationService.AuthorizeAsync(user, QueryPermissions.CreatePermissionForQuery(query.Name));

    private DataSetDescriptor Describe(Query query)
        => new(query.Name, query.Name)
        {
            Group = query.Source,
            Description = query.ReturnContentItems ? S["A {0} query that returns content items.", query.Source] : S["A {0} query.", query.Source],
        };

    private DataField[] ContentFields()
        =>
        [
            new("ContentItemId", S["Content item id"], DataFieldType.Text) { IsIdentifier = true },
            new("ContentItemVersionId", S["Version id"], DataFieldType.Text) { IsIdentifier = true },
            new("ContentType", S["Content type"], DataFieldType.Text),
            new("DisplayText", S["Display text"], DataFieldType.Text),
            new("Owner", S["Owner"], DataFieldType.Text) { IsIdentifier = true },
            new("Author", S["Author"], DataFieldType.Text),
            new("Published", S["Published"], DataFieldType.Boolean),
            new("Latest", S["Latest"], DataFieldType.Boolean),
            new("CreatedUtc", S["Created"], DataFieldType.DateTime),
            new("ModifiedUtc", S["Modified"], DataFieldType.DateTime),
            new("PublishedUtc", S["Published on"], DataFieldType.DateTime),
        ];

    private static object[] ToContentRow(ContentItem item)
        =>
        [
            item.ContentItemId,
            item.ContentItemVersionId,
            item.ContentType,
            item.DisplayText,
            item.Owner,
            item.Author,
            item.Published,
            item.Latest,
            item.CreatedUtc,
            item.ModifiedUtc,
            item.PublishedUtc,
        ];
}
