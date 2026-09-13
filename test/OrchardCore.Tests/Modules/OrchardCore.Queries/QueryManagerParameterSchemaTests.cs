using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Documents;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Core.Models;
using OrchardCore.Queries.Core.Services;
using OrchardCore.Queries.Sql;
using OrchardCore.Queries.Sql.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Queries;

public class QueryManagerParameterSchemaTests
{
    [Fact]
    public async Task SaveAsync_PersistsParameterSchema()
    {
        var services = new ServiceCollection()
            .AddKeyedSingleton<IQuerySource>(SqlQuerySource.SourceName, new TestQuerySource(SqlQuerySource.SourceName))
            .BuildServiceProvider();
        var documentManager = new TestDocumentManager();
        var queryManager = new DefaultQueryManager(documentManager, [], NullLogger<DefaultQueryManager>.Instance, services);

        var parameterSchema = "{\"type\":\"object\",\"properties\":{\"size\":{\"type\":\"integer\"}}}";
        var resultSchema = "{\"type\":\"object\",\"properties\":{\"Name\":{\"type\":\"string\"}}}";
        var data = new JsonObject
        {
            [nameof(Query.Name)] = "RemoteApiQuery",
            [nameof(Query.Source)] = SqlQuerySource.SourceName,
            [nameof(Query.Schema)] = resultSchema,
            [nameof(Query.ParameterSchema)] = parameterSchema,
            [nameof(Query.Properties)] = new JsonObject
            {
                [nameof(SqlQueryMetadata)] = new JsonObject
                {
                    [nameof(SqlQueryMetadata.Template)] = "select 1 as DocumentId",
                },
            },
        };

        var query = await queryManager.NewAsync(SqlQuerySource.SourceName, data);
        await queryManager.SaveAsync(query);

        var stored = await queryManager.GetQueryAsync("RemoteApiQuery");

        Assert.Equal(parameterSchema, stored.ParameterSchema);
        Assert.Equal(resultSchema, stored.Schema);
        Assert.Equal(SqlQuerySource.SourceName, stored.Source);
    }

    private sealed class TestDocumentManager : IDocumentManager<QueriesDocument>
    {
        private QueriesDocument _document = new();

        public Task<QueriesDocument> GetOrCreateMutableAsync(Func<Task<QueriesDocument>> factoryAsync = null) =>
            Task.FromResult(_document);

        public Task<QueriesDocument> GetOrCreateImmutableAsync(Func<Task<QueriesDocument>> factoryAsync = null) =>
            Task.FromResult(_document);

        public Task UpdateAsync(QueriesDocument document, Func<QueriesDocument, Task> afterUpdateAsync = null)
        {
            _document = document;
            return afterUpdateAsync?.Invoke(document) ?? Task.CompletedTask;
        }
    }

    private sealed class TestQuerySource : IQuerySource
    {
        public TestQuerySource(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters) =>
            throw new NotSupportedException();
    }
}
