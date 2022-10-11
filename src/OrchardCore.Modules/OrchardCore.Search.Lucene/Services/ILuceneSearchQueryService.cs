using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.Services
{
    [Obsolete("This interface has been deprecated and we will be removed in the next major release, please use OrchardCore.Search.Lucene.Abstractions instead.", false)]
    public interface ILuceneSearchQueryService
    {
        /// <summary>
        /// Provides a way to execute a search request in Lucene based on a Lucene Query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="indexName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns><see cref="IList{String}">IList&lt;string&gt;</see></returns>
        Task<IList<string>> ExecuteQueryAsync(Query query, string indexName, int start = 0, int end = 20);
    }
}
