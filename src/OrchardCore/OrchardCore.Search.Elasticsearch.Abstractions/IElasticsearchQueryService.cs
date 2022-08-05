using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticsearchQueryService
    {
        /// <summary>
        /// <para>Provides a way to execute a search request in Elasticsearch based on a JSON string.</para>
        /// <para>OC implementation deserializes that JSON string to an Elasticsearch SearchRequest.</para>
        /// <para>Also provides a way to return only specific fields:
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/7.17/search-fields.html#search-fields">The fields option</see>.
        /// </para>
        /// <para>Also provides a way to return only specific _source fields:
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/7.17/search-fields.html#source-filtering">The _source option</see>.
        /// </para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns><see cref="ElasticsearchTopDocs"/></returns>
        Task<ElasticsearchTopDocs> SearchAsync(ElasticsearchQueryContext context, string query);
    }
}
