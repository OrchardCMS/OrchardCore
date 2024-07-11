using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

using ElasticSearchDescriptor = SearchDescriptor<Dictionary<string, object>>;

public interface IElasticIndexManager
{
    Task<bool> CreateIndexAsync(string indexName, string analyzerName, bool storeSourceData);

    Task<bool> DeleteIndex(string indexName);

    Task<bool> ExistsAsync(string indexName);

    Task<string> GetIndexMappings(string indexName);

    Task<ElasticTopDocs> SearchAsync(string indexName, Func<ElasticSearchDescriptor, ElasticSearchDescriptor> selector);
 
    Task<ElasticSearchDescriptor> DeserializeSearchDescriptor(string request);

    Task<long> GetLastTaskId(string indexName);

    Task<bool> SetLastTaskId(string indexName, long lastTaskId);

    Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments);

    Task<bool> DeleteAllDocumentsAsync(string indexName);

    Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds);
}
