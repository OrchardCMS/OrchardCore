using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Orchard.Queries
{
    public interface IQueryManager
    {
        /// <summary>
        /// Returns a list of all store <see cref="Query"/>.
        /// </summary>
        Task<IEnumerable<Query>> ListQueriesAsync();

        /// <summary>
        /// Saves the specific <see cref="Query"/>.
        /// </summary>
        /// <param name="query">The <see cref="Query"/> instance to save.</param>
        Task SaveQueryAsync(Query query);

        /// <summary>
        /// Deletes the specified <see cref="Query"/>.
        /// </summary>
        /// <param name="name">The name of the query to delete.</param>
        Task DeleteQueryAsync(string name);

        /// <summary>
        /// Gets the <see cref="Query"/> instance with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<Query> GetQueryAsync(string name);

        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        Task<JToken> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
    }

    public static class QueryManagerExtensions
    {
        public static Task<JToken> ExecuteQueryAsync(this IQueryManager queryManager, Query query, JObject obj)
        {
            var parameters = obj == null
                ? new Dictionary<string, object>()
                : obj.ToDictionary<KeyValuePair<string, JToken>, string, object>(x => x.Key, y =>
            {
                switch (y.Value.Type)
                {
                    case JTokenType.Boolean:
                        return y.Value.ToObject<bool>();
                    case JTokenType.String:
                        return y.Value.ToObject<string>();
                    case JTokenType.Float:
                        return y.Value.ToObject<float>();
                }

                return y.Value.ToString();
            });

            return queryManager.ExecuteQueryAsync(query, parameters);
        }
    }
}
