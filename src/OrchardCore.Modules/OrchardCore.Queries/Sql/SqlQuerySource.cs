using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.Entities;
using OrchardCore.Json;
using OrchardCore.Liquid;
using OrchardCore.Queries.Sql.Models;
using YesSql;

namespace OrchardCore.Queries.Sql;

public sealed class SqlQuerySource : IQuerySource
{
    public const string SourceName = "Sql";

    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TemplateOptions _templateOptions;

    public SqlQuerySource(
        ILiquidTemplateManager liquidTemplateManager,
        IDbConnectionAccessor dbConnectionAccessor,
        ISession session,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IOptions<TemplateOptions> templateOptions,
        ILogger<SqlQuerySource> logger)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _dbConnectionAccessor = dbConnectionAccessor;
        _session = session;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _templateOptions = templateOptions.Value;
        _logger = logger;
    }

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var metadata = query.As<SqlQueryMetadata>();

        var sqlQueryResults = new SQLQueryResults
        {
            Items = []
        };

        var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(metadata.Template, NullEncoder.Default,
            parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

        var configuration = _session.Store.Configuration;

        if (!SqlParser.TryParse(
                tokenizedQuery,
                configuration.Schema,
                configuration.SqlDialect,
                configuration.TablePrefix,
                parameters,
                out var rawQuery,
                out var messages))
        {
            _logger.LogError("Couldn't parse SQL query: {Messages}", string.Join(' ', messages));

            return sqlQueryResults;
        }

        await using var connection = _dbConnectionAccessor.CreateConnection();

        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync(configuration.IsolationLevel);

        var queryResults = await connection.QueryAsync(rawQuery, parameters, transaction);

        if (!query.ReturnContentItems)
        {
            sqlQueryResults.Items = queryResults
                .Select<object, JsonObject>(document => JObject.FromObject(document, _jsonSerializerOptions))
                .ToArray();

            return sqlQueryResults;
        }

        string column = null;

        var documentIds = queryResults
            .Select(row =>
            {
                var rowDictionary = (IDictionary<string, object>)row;

                if (column == null)
                {
                    if (rowDictionary.ContainsKey(nameof(ContentItemIndex.DocumentId)))
                    {
                        column = nameof(ContentItemIndex.DocumentId);
                    }
                    else
                    {
                        column = rowDictionary.FirstOrDefault(kv => kv.Value is long).Key
                            ?? rowDictionary.First().Key;
                    }
                }

                if (rowDictionary.TryGetValue(column, out var documentIdObject))
                {
                    return documentIdObject switch
                    {
                        long longValue => longValue,
                        int intValue => intValue,
                        { } otherObject =>
                            long.TryParse(otherObject.ToString(), CultureInfo.InvariantCulture, out var parsedValue)
                                ? parsedValue
                                : 0,
                        _ => 0,
                    };
                }

                return 0;
            }).ToArray();

        if (documentIds.Length > 0)
        {
            sqlQueryResults.Items = await _session.GetAsync<ContentItem>(documentIds);
        }

        return sqlQueryResults;
    }
}
