using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nest;

using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elastic.Services;

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
        private ConcurrentDictionary<string, DateTime> _timestamps = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ElasticIndexManager(
            IClock clock,
            ILogger<ElasticIndexManager> logger,
            ElasticAnalyzerManager elasticAnalyzerManager,
            ElasticIndexSettingsService elasticIndexSettingsService,
            IElasticClient elasticClient
            )
        {
            
            _clock = clock;
            _logger = logger;
            _elasticAnalyzerManager = elasticAnalyzerManager;
            _elasticIndexSettingsService = elasticIndexSettingsService;
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
            List<ElasticDocument> documents = new List<ElasticDocument>();
            foreach (var contentItemId in contentItemIds)
            {
                documents.Add(CreateElasticDocument(contentItemId));
            }
            var result = await _elasticClient.DeleteManyAsync(documents, indexName);
            if (result.Errors)
            {
                _logger.LogWarning($"There were issues deleting documents from Elastic search. {result.OriginalException}");
            }
            return result.IsValid;
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
            if (string.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }
            var existResponse = await _elasticClient.Indices.ExistsAsync(indexName);
            return existResponse.Exists;
        }

        public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            //Convert Document to a structure suitable for Elastic
            List<ElasticDocument> documents = new List<ElasticDocument>();
            foreach (var indexDocument in indexDocuments)
            {
                documents.Add(CreateElasticDocument(indexDocument));
            }
            var result = await _elasticClient.IndexManyAsync(documents, indexName);
            if (result.Errors)
            {
                _logger.LogWarning($"There were issues reported indexing the documents. {result.ServerError}");
            }
        }

        public async Task<ElasticTopDocs> SearchAsync(string indexName, string query)
        {
            ElasticTopDocs elasticTopDocs = new ElasticTopDocs();
            if (await Exists(indexName))
            {
                var searchResponse = await _elasticClient.SearchAsync<ElasticDocument>(s => s
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

        private ElasticDocument CreateElasticDocument(DocumentIndex documentIndex)
        {
            ElasticDocument elasticDocument = new ElasticDocument(documentIndex.ContentItemId.ToLowerInvariant());
            foreach (var entry in documentIndex.Entries)
            {
                if (entry.Name.Contains("Analyzed") || entry.Name.Contains("Sanitize") || entry.Name.Contains("Normalized"))
                {
                    continue;
                }

                switch (entry.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // store "true"/"false" for booleans
                        elasticDocument.Set(entry.Name, (bool)(entry.Value));
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value != null)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                elasticDocument.Set(entry.Name, ((DateTimeOffset)(entry.Value)).UtcDateTime);
                            }
                            else
                            {
                                elasticDocument.Set(entry.Name, ((DateTime)(entry.Value)).ToUniversalTime());
                            }
                        }
                        //else
                        //{
                        //    elasticDocument.Set(entry.Name, null);
                        //}
                        break;

                    case DocumentIndex.Types.Integer:
                        if (entry.Value != null && Int32.TryParse(entry.Value.ToString(), out var value))
                        {
                            elasticDocument.Set(entry.Name, Convert.ToInt32(entry.Value));
                        }
                        //else
                        //{
                        //    elasticDocument.Set(entry.Name, null);
                        //}

                        break;

                    case DocumentIndex.Types.Number:
                        if (entry.Value != null)
                        {
                            elasticDocument.Set(entry.Name, Convert.ToDouble(entry.Value));
                        }
                        //else
                        //{
                        //    elasticDocument.Set(entry.Name, null);
                        //}
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Value != null && !String.IsNullOrEmpty(Convert.ToString(entry.Value)))
                        {
                            elasticDocument.Set(entry.Name, Convert.ToString(entry.Value));
                        }
                        //else
                        //{
                        //    elasticDocument.Set(entry.Name, null);
                        //}
                        break;
                }

            }
            return elasticDocument;
        }
        private ElasticDocument CreateElasticDocument(string contentItemId)
        {
            return new ElasticDocument(contentItemId);
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
