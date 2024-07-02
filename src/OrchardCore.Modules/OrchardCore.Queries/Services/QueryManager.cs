using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Queries.Indexes;
using YesSql;

namespace OrchardCore.Queries.Services
{
    public class QueryManager : IQueryManager
    {
        private readonly IEnumerable<IQuerySource> _querySources;
        private readonly ISession _session;

        public QueryManager(
            IEnumerable<IQuerySource> querySources,
            ISession session)
        {
            _querySources = querySources;
            _session = session;
        }

        public async Task DeleteQueryAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            await DeleteQueryInternalAsync(name, true);
        }

        public async Task<Query> GetQueryAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var query = await _session.Query<Query, QueryIndex>(x => x.Name == name).FirstOrDefaultAsync();

            return query;
        }

        public async Task<QueryPageResult> ListQueriesAsync(Func<QueryIndex, bool> predicate = null, int? page = null, int? pageSize = null)
        {
            var query = _session.Query<Query, QueryIndex>();

            if (predicate != null)
            {
                query = query.Where(q => predicate(q));
            }

            query = query
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id);

            if (page == null || page < 0)
            {
                page = 1;
            }

            var count = await query.CountAsync();

            if (pageSize > 0)
            {
                return new QueryPageResult()
                {
                    Count = count,
                    Queries = await query.Take(pageSize.Value)
                    .Skip((page.Value - 1) * pageSize.Value)
                    .ListAsync(),
                };
            }

            return new QueryPageResult()
            {
                Count = count,
                Queries = await query.ListAsync(),
            };
        }

        public async Task SaveQueryAsync(string name, Query query)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            ArgumentNullException.ThrowIfNull(query);

            await DeleteQueryInternalAsync(name, false);

            await _session.SaveAsync(query);
        }

        public Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var querySource = _querySources.FirstOrDefault(q => q.Name == query.Source)
                ?? throw new ArgumentException("Query source not found: " + query.Source);

            return querySource.ExecuteQueryAsync(query, parameters);
        }

        private async Task DeleteQueryInternalAsync(string name, bool commit)
        {
            var queries = await _session.Query<Query, QueryIndex>(x => x.Name == name).ListAsync();

            foreach (var query in queries)
            {
                _session.Delete(query);
            }

            if (commit && queries.Any())
            {
                await _session.SaveChangesAsync();
            }
        }
    }
}
