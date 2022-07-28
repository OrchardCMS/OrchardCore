using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Search.Elastic
{
    /// <summary>
    /// Provides methods to manage physical Lucene indices.
    /// This class is provided as a singleton to that the index searcher can be reused across requests.
    /// </summary>
    public class ElasticIndexManager : IDisposable
    {
        private readonly IElasticClient _elasticClient;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private bool _disposing;
        private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

        private readonly string[] IgnoredFields = {
            "Analyzed",
            "Sanitize",
            "Normalized",
            "Inherited"
        };

        public ElasticIndexManager(
            IClock clock,
            ILogger<ElasticIndexManager> logger,
            IElasticClient elasticClient
            )
        {

            _clock = clock;
            _logger = logger;
            _elasticClient = elasticClient;
        }

        public async Task<bool> CreateIndexAsync(string indexName)
        {
            //Get Index name scoped by ShellName
            if (!await Exists(indexName))
            {
                var response = await _elasticClient.Indices.CreateAsync(indexName);

                return response.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
        {
            var success = true;

            if (contentItemIds.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var id in contentItemIds)
                    descriptor.Delete<Dictionary<string,object>>(d => d
                        .Index(indexName)
                        .Id(id)
                    );

                var response = await _elasticClient.BulkAsync(descriptor);

                if (response.Errors)
                {
                    _logger.LogWarning("There were issues deleting documents from Elastic search. {result.OriginalException}", response.OriginalException);
                }

                success = response.IsValid;
            }

            return success;
        }

        public async Task<bool> DeleteIndex(string indexName)
        {
            if (await Exists(indexName))
            {
                var result = await _elasticClient.Indices.DeleteAsync(indexName);
                return result.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            var existResponse = await _elasticClient.Indices.ExistsAsync(indexName);

            return existResponse.Exists;
        }

        public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            //Convert Document to a structure suitable for Elastic
            var documents = new List<Dictionary<string, object>>();

            foreach (var indexDocument in indexDocuments)
            {
                documents.Add(CreateElasticDocument(indexDocument));
            }

            if (documents.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var document in documents)
                {
                    descriptor.Index<Dictionary<string, object>>(op => op
                        .Id(document.GetValueOrDefault("ContentItemId").ToString())
                        .Document(document)
                        .Index(indexName)
                    );
                }

                var result = await _elasticClient.BulkAsync(d => descriptor);

                if (result.Errors)
                {
                    _logger.LogWarning("There were issues reported indexing the documents. {result.ServerError}", result.ServerError);
                }
            }
        }

        public async Task<ElasticTopDocs> SearchAsync(string indexName, string query)
        {
            var elasticTopDocs = new ElasticTopDocs();

            if (await Exists(indexName))
            {
                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(s => s
                    .Index(indexName)
                    .Query(q => new RawQuery(query))
                    );

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Documents.Count;
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                }

                _timestamps[indexName] = _clock.UtcNow;
            }

            return elasticTopDocs;
        }

        public async Task<ElasticTopDocs> SearchAsync(string indexName, QueryContainer query, int from, int size)
        {
            var elasticTopDocs = new ElasticTopDocs();

            if (await Exists(indexName))
            {
                var searchRequest = new SearchRequest(indexName)
                {
                    Query = query,
                    From = from,
                    Size = size
                };

                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchRequest);

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Documents.Count;
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                }

                _timestamps[indexName] = _clock.UtcNow;
            }

            return elasticTopDocs;
        }

        public async Task SearchAsync(string indexName, Func<IElasticClient, Task> elasticClient)
        {
            if (await Exists(indexName))
            {
                await elasticClient(_elasticClient);

                _timestamps[indexName] = _clock.UtcNow;
            }
        }

        private Dictionary<string, object> CreateElasticDocument(DocumentIndex documentIndex)
        {
            var entries = new Dictionary<string, object>
            {
                { "ContentItemId", documentIndex.ContentItemId },
                { "ContentItemVersionId", documentIndex.ContentItemVersionId }
            };

            foreach (var entry in documentIndex.Entries)
            {
                if (entries.ContainsKey(entry.Name)
                    || Array.Exists(IgnoredFields, x => entry.Name.Contains(x)))
                {
                    continue;
                }

                switch (entry.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // store "true"/"false" for booleans
                        entries.Add(entry.Name, (bool)(entry.Value));
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value != null)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                entries.Add(entry.Name, ((DateTimeOffset)(entry.Value)).UtcDateTime);
                            }
                            else
                            {
                                entries.Add(entry.Name, ((DateTime)(entry.Value)).ToUniversalTime());
                            }
                        }
                        break;

                    case DocumentIndex.Types.Integer:
                        if (entry.Value != null && Int64.TryParse(entry.Value.ToString(), out var value))
                        {
                            entries.Add(entry.Name, value);
                        }

                        break;

                    case DocumentIndex.Types.Number:
                        if (entry.Value != null)
                        {
                            entries.Add(entry.Name, Convert.ToDouble(entry.Value));
                        }
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Value != null && !String.IsNullOrEmpty(Convert.ToString(entry.Value)))
                        {
                            entries.Add(entry.Name, Convert.ToString(entry.Value));
                        }
                        break;
                }

            }
            return entries;
        }

        private static Dictionary<string, object> CreateElasticDocument(string contentItemId)
        {
            var entries = new Dictionary<string, object>
            {
                { "Id", contentItemId }
            };

            return entries;
        }

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
        }

        ~ElasticIndexManager()
        {
            Dispose();
        }
    }
}
