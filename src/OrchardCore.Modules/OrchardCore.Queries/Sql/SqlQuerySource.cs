using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Fluid;
using Fluid.Values;
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
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TemplateOptions _templateOptions;

    public SqlQuerySource(
        ILiquidTemplateManager liquidTemplateManager,
        IDbConnectionAccessor dbConnectionAccessor,
        ISession session,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IOptions<TemplateOptions> templateOptions)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _dbConnectionAccessor = dbConnectionAccessor;
        _session = session;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _templateOptions = templateOptions.Value;
    }

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var metadata = query.As<SqlQueryMetadata>();

        var sqlQueryResults = new SQLQueryResults();

        var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(metadata.Template, NullEncoder.Default,
            parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

        var dialect = _session.Store.Configuration.SqlDialect;

        if (!SqlParser.TryParse(tokenizedQuery, _session.Store.Configuration.Schema, dialect, _session.Store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
        {
            sqlQueryResults.Items = [];

            return sqlQueryResults;
        }

        await using var connection = _dbConnectionAccessor.CreateConnection();

        await connection.OpenAsync();

        if (query.ReturnContentItems)
        {
            using var transaction = await connection.BeginTransactionAsync(_session.Store.Configuration.IsolationLevel);
            var queryResult = await connection.QueryAsync(rawQuery, parameters, transaction);

            string column = null;

            var documentIds = queryResult.Select(row =>
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

                return rowDictionary.TryGetValue(column, out var documentIdObject) && documentIdObject is long documentId
                    ? documentId
                    : 0;
            }).ToArray();

            sqlQueryResults.Items = await _session.GetAsync<ContentItem>(documentIds);

            return sqlQueryResults;
        }
        else
        {
            using var transaction = await connection.BeginTransactionAsync(_session.Store.Configuration.IsolationLevel);
            var queryResults = await connection.QueryAsync(rawQuery, parameters, transaction);

            var results = new List<JsonObject>();
            foreach (var document in queryResults)
            {
                results.Add(JObject.FromObject(document, _jsonSerializerOptions));
            }

            sqlQueryResults.Items = results;

            return sqlQueryResults;
        }
    }
}
