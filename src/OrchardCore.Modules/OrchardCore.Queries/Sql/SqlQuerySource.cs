using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Fluid;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly IStore _store;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;

        public SqlQuerySource(
            IStore store,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session)
        {
            _store = store;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
        }

        public string Name => "Sql";

        public Query Create()
        {
            return new SqlQuery();
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var sqlQuery = query as SqlQuery;
            var sqlQueryResults = new SQLQueryResults();

            var templateContext = new TemplateContext();

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    templateContext.SetValue(parameter.Key, parameter.Value);
                }
            }

            var tokenizedQuery = await _liquidTemplateManager.RenderAsync(sqlQuery.Template, NullEncoder.Default, templateContext);

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            var dialect = SqlDialectFactory.For(connection);

            if (!SqlParser.TryParse(tokenizedQuery, dialect, _store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
            {
                sqlQueryResults.Items = new object[0];
                connection.Dispose();
                return sqlQueryResults;
            }

            if (sqlQuery.ReturnDocuments)
            {
                IEnumerable<int> documentIds;

                using (connection)
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel))
                    {
                        documentIds = await connection.QueryAsync<int>(rawQuery, parameters);
                    }
                }

                sqlQueryResults.Items = await _session.GetAsync<ContentItem>(documentIds.ToArray());
                return sqlQueryResults;
            }
            else
            {
                IEnumerable<dynamic> queryResults;

                using (connection)
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel))
                    {
                        queryResults = await connection.QueryAsync(rawQuery, parameters);
                    }
                }

                var results = new List<JObject>();

                foreach (var document in queryResults)
                {
                    results.Add(JObject.FromObject(document));
                }

                sqlQueryResults.Items = results;
                return sqlQueryResults;
            }
        }
    }
}
