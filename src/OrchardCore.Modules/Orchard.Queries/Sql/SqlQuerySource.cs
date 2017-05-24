using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Orchard.Tokens.Services;
using YesSql;

namespace Orchard.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly IStore _store;
        private readonly ITokenizer _tokenizer;

        public SqlQuerySource(
            IStore store,
            ITokenizer tokenizer)
        {
            _store = store;
            _tokenizer = tokenizer;
        }

        public string Name => "Sql";

        public Query Create()
        {
            return new SqlQuery();
        }

        public async Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var sqlQuery = query as SqlQuery;
            object result = null;

            var tokenizedQuery = _tokenizer.Tokenize(sqlQuery.Template, parameters);

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            var dialect = SqlDialectFactory.For(connection);

            if (SqlParser.TryParse(sqlQuery.Template, _store.Configuration.TablePrefix, out var rawQuery, out var rawParameters, out var messages))
            {
                using (connection)
                {
                    connection.Open();
                    result = await connection.QueryAsync(rawQuery, rawParameters);
                }
            }

            return result;
        }
    }
}
