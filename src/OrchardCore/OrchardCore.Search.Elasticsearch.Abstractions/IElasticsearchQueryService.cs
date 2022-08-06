using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticSearchQueryService
    {
        /// <summary>
        /// Provides a way to execute a search request in Elasticsearch based on a QueryContainer. 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns><see cref="IList{String}">IList&lt;string&gt;</see></returns>
        Task<IList<string>> ExecuteQueryAsync(string indexName, QueryContainer query, List<ISort> sort, int start = 0, int end = 20);
    }
}
