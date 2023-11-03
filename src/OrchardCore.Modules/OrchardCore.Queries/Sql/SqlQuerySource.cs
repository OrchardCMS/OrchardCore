namespace OrchardCore.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IDbConnectionAccessor _dbConnectionAccessor;
        private readonly ISession _session;
        private readonly TemplateOptions _templateOptions;

        public SqlQuerySource(
            ILiquidTemplateManager liquidTemplateManager,
            IDbConnectionAccessor dbConnectionAccessor,
            ISession session,
            IOptions<TemplateOptions> templateOptions)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _dbConnectionAccessor = dbConnectionAccessor;
            _session = session;
            _templateOptions = templateOptions.Value;
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

            var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(sqlQuery.Template, NullEncoder.Default,
                parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

            var dialect = _session.Store.Configuration.SqlDialect;

            if (!SqlParser.TryParse(tokenizedQuery, _session.Store.Configuration.Schema, dialect, _session.Store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
            {
                sqlQueryResults.Items = Array.Empty<object>();

                return sqlQueryResults;
            }
            var results = new List<JObject>();

            await using var connection = _dbConnectionAccessor.CreateConnection();
            try
            {
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction(_session.Store.Configuration.IsolationLevel);

                if (sqlQuery.ReturnDocuments)
                {
                    var documentIds = await connection.QueryAsync<long>(rawQuery, parameters, transaction);

                    sqlQueryResults.Items = await _session.GetAsync<ContentItem>(documentIds.ToArray());

                    return sqlQueryResults;
                }

                var queryResults = await connection.QueryAsync(rawQuery, parameters, transaction);

                foreach (var document in queryResults)
                {
                    results.Add(JObject.FromObject(document));
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
            sqlQueryResults.Items = results;

            return sqlQueryResults;
        }
    }
}
