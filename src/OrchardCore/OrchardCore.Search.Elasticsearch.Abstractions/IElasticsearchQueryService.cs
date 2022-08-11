using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticSearchQueryService
    {
        /// <summary>
        /// Provides a way to execute a search request in Elasticsearch based on a <see cref="QueryContainer"/>. 
        /// </summary>
        /// <returns><see cref="IList{String}">IList&lt;string&gt;</see></returns>
        Task<IList<string>> ExecuteQueryAsync(string indexName, QueryContainer query, List<ISort> sort, int start = 0, int end = 20);
    }
}
