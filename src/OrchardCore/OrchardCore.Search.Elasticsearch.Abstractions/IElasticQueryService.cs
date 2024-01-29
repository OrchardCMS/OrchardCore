using System.Threading.Tasks;
using OrchardCore.Queries;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticQueryService
    {
        /// <summary>
        /// <para>Provides a way to execute an OC <see cref="Query"/> in Elasticsearch.</para>
        /// <para>OC implementation deserializes the <see cref="Query"/> JSON string to an Elasticsearch SearchRequest.</para>
        /// <para>Also provides a way to return only specific fields:
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/7.17/search-fields.html#search-fields">The fields option</see>.
        /// </para>
        /// <para>Also provides a way to return only specific _source fields:
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/7.17/search-fields.html#source-filtering">The _source option</see>.
        /// </para>
        /// </summary>
        /// <returns><see cref="ElasticTopDocs"/></returns>
        Task<ElasticTopDocs> SearchAsync(string indexName, string query);
    }
}
