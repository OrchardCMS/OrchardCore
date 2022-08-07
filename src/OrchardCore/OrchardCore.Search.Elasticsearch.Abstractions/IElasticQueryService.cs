using System.Threading.Tasks;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticQueryService
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
        /// <returns><see cref="ElasticTopDocs"/></returns>
        Task<ElasticTopDocs> SearchAsync(ElasticQueryContext context, string query);
    }
}
