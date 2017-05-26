using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Tokens.Services;
using YesSql;
using YesSql.Services;

namespace Orchard.Queries.Sql
{
    public class SqlQuerySource : IQuerySource
    {
        private readonly IStore _store;
        private readonly ITokenizer _tokenizer;
        private readonly ISession _session;

        public SqlQuerySource(
            IStore store,
            ITokenizer tokenizer,
            ISession session)
        {
            _store = store;
            _tokenizer = tokenizer;
            _session = session;
        }

        public string Name => "Sql";

        public Query Create()
        {
            return new SqlQuery();
        }

        public async Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var sqlQuery = query as SqlQuery;

            var tokenizedQuery = _tokenizer.Tokenize(sqlQuery.Template, parameters);

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            var dialect = SqlDialectFactory.For(connection);

            if (SqlParser.TryParse(sqlQuery.Template, dialect, _store.Configuration.TablePrefix, out var rawQuery, out var rawParameters, out var messages))
            {
                
            }

            var results = new List<JObject>();

            
            if (sqlQuery.ReturnContentItems)
            {
                IEnumerable<string> contentItemVersionIds;

                using (connection)
                {
                    connection.Open();
                    contentItemVersionIds = await connection.QueryAsync<string>(rawQuery, rawParameters);
                }

                var contentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(contentItemVersionIds)).ListAsync();

                // Reorder the result to preserve the one from the lucene query
                var indexed = contentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                return contentItemVersionIds.Select(x => indexed[x]).ToArray();
            }
            else
            {
                IEnumerable<dynamic> queryResults;

                using (connection)
                {
                    connection.Open();
                    queryResults = await connection.QueryAsync(rawQuery, rawParameters);
                }

                foreach (var document in queryResults)
                {
                    results.Add(JObject.FromObject(document));
                }

                return results;
            }
        }
    }
}
