using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using LuceneNetCodecs = Lucene.Net.Codecs;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;
using Orchard.Indexing;
using Directory = System.IO.Directory;

namespace Orchard.Lucene
{
    public class LuceneIndexProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _rootPath;
        private readonly DirectoryInfo _rootDirectory;

        private static LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;

        static LuceneIndexProvider()
        {
            SPIClassIterator<LuceneNetCodecs.Codec>.Types.Add(typeof(LuceneNetCodecs.Lucene46.Lucene46Codec));
            SPIClassIterator<LuceneNetCodecs.PostingsFormat>.Types.Add(typeof(LuceneNetCodecs.Lucene41.Lucene41PostingsFormat));
            SPIClassIterator<LuceneNetCodecs.DocValuesFormat>.Types.Add(typeof(LuceneNetCodecs.Lucene45.Lucene45DocValuesFormat));
        }

        public LuceneIndexProvider(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _hostingEnvironment = hostingEnvironment;
            _rootPath = Path.Combine(shellOptions.Value.ShellsRootContainerName, shellOptions.Value.ShellsContainerName, shellSettings.Name, "Lucene");
            _rootDirectory = Directory.CreateDirectory(_rootPath);
        }

        public void CreateIndex(string indexName)
        {
            var path = new DirectoryInfo(Path.Combine(_rootPath, indexName));

            if (!path.Exists)
            {
                path.Create();
            }

            using (var directory = FSDirectory.Open(path))
            using (var iwriter = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion, new StandardAnalyzer(LuceneVersion))))
            {

            }
        }

        public void DeleteDocuments(string indexName, IEnumerable<string> contentItemIds)
        {
            // FOR DEBUG ONLY
            //foreach (var documentId in documentIds)
            //{
            //    var filename = GetFilename(indexName, documentId);
            //    if (File.Exists(filename))
            //    {
            //        File.Delete(filename);
            //    }
            //}

            var path = new DirectoryInfo(Path.Combine(_rootPath, indexName));

            using (var directory = FSDirectory.Open(path))
            using (var iwriter = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48))))
            {
                iwriter.DeleteDocuments(contentItemIds.Select(x => new Term("ContentItemId", x)).ToArray());
            }
        }

        public void DeleteIndex(string indexName)
        {
            var indexFolder = Path.Combine(_rootPath, indexName);

            if (Directory.Exists(indexFolder))
            {
                Directory.Delete(indexFolder, true);
            }
        }

        public bool Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            return Directory.Exists(Path.Combine(_rootPath, indexName));
        }

        public int GetLastIndexDocumentId(string indexName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> List()
        {
            return _rootDirectory
                .GetDirectories()
                .Select(x => x.Name);
        }

        public void StoreDocuments(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            // FOR DEBUG ONLY
            //foreach(var indexDocument in indexDocuments)
            //{
            //    var filename = GetFilename(indexName, indexDocument.DocumentId);
            //    var content = JsonConvert.SerializeObject(indexDocument);
            //    File.WriteAllText(filename, content);
            //}

            var path = new DirectoryInfo(Path.Combine(_rootPath, indexName));

            if (!path.Exists)
            {
                path.Create();
            }

            using (var directory = FSDirectory.Open(path))
            {
                using (var iwriter = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48))))
                {
                    foreach (var indexDocument in indexDocuments)
                    {
                        iwriter.AddDocument(CreateLuceneDocument(indexDocument));
                    }
                }
            }
        }

        public void Search(string indexName, Action<IndexSearcher> searcher)
        {
            var path = new DirectoryInfo(Path.Combine(_rootPath, indexName));

            using (var directory = FSDirectory.Open(path))
            {
                using (var indexReader = DirectoryReader.Open(directory))
                {
                    var iSearcher = new IndexSearcher(indexReader);
                    searcher(iSearcher);
                }
            }
        }

        public void Read(string indexName, Action<DirectoryReader> reader)
        {
            var path = new DirectoryInfo(Path.Combine(_rootPath, indexName));

            using (var directory = FSDirectory.Open(path))
            {
                using (var indexReader = DirectoryReader.Open(directory))
                {
                    reader(indexReader);
                }
            }
        }

        private Document CreateLuceneDocument(DocumentIndex documentIndex)
        {
            var doc = new Document();

            // Always store the content item id
            doc.Add(new StringField("ContentItemId", documentIndex.ContentItemId.ToString(), Field.Store.YES));

            foreach (var entry in documentIndex.Entries)
            {
                var store = entry.Value.Options.HasFlag(DocumentIndexOptions.Store)
                            ? Field.Store.YES
                            : Field.Store.NO
                            ;

                if (entry.Value.Value == null)
                {
                    continue;
                }

                switch (entry.Value.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // store "true"/"false" for booleans
                        doc.Add(new StringField(entry.Key, Convert.ToString(entry.Value.Value).ToLowerInvariant(), store));
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value.Value is DateTimeOffset)
                        {
                            doc.Add(new StringField(entry.Key, ((DateTimeOffset)entry.Value.Value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), store));
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Key, ((DateTime)entry.Value.Value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), store));
                        }
                        break;

                    case DocumentIndex.Types.Integer:
                        doc.Add(new IntField(entry.Key, Convert.ToInt32(entry.Value.Value), store));
                        break;

                    case DocumentIndex.Types.Number:
                        doc.Add(new DoubleField(entry.Key, Convert.ToDouble(entry.Value.Value), store));
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Value.Options.HasFlag(DocumentIndexOptions.Analyze))
                        {
                            doc.Add(new TextField(entry.Key, Convert.ToString(entry.Value.Value), store));
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Key, Convert.ToString(entry.Value.Value), store));
                        }
                        break;
                }

            }

            return doc;
        }

        private string GetFilename(string indexName, int documentId)
        {
            return Path.Combine(_rootPath, indexName, documentId + ".json");
        }
    }
}
