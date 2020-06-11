using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Fluid;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IDbConnectionAccessor _dbConnectionAccessor;
        private readonly ISession _session;

        public SqlQuerySource(
            ILiquidTemplateManager liquidTemplateManager,
            IDbConnectionAccessor dbConnectionAccessor,
            ISession session)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _dbConnectionAccessor = dbConnectionAccessor;
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

            var templateContext = _liquidTemplateManager.Context;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    templateContext.SetValue(parameter.Key, parameter.Value);
                }
            }

            var tokenizedQuery = await _liquidTemplateManager.RenderAsync(sqlQuery.Template, NullEncoder.Default);

            var connection = _dbConnectionAccessor.CreateConnection();
            var dialect = SqlDialectFactory.For(connection);

            if (!SqlParser.TryParse(tokenizedQuery, dialect, _session.Store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
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

                    using (var transaction = connection.BeginTransaction(_session.Store.Configuration.IsolationLevel))
                    {
                        documentIds = await connection.QueryAsync<int>(rawQuery, parameters, transaction);
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

                    using (var transaction = connection.BeginTransaction(_session.Store.Configuration.IsolationLevel))
                    {
                        queryResults = await connection.QueryAsync(rawQuery, parameters, transaction);
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
