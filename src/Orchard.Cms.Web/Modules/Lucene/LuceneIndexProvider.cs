using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;
using Orchard.Indexing;
using Directory = System.IO.Directory;

namespace Lucene
{
    public class LuceneIndexProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _rootPath;
        private readonly DirectoryInfo _rootDirectory;

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
            using (var directory = new NIOFSDirectory(_rootDirectory.CreateSubdirectory(indexName)))
            {
            }
        }

        public void DeleteDocuments(string indexName, IEnumerable<int> documentIds)
        {
        }

        public void DeleteIndex(string indexName)
        {
            Directory.Delete(Path.Combine(_rootPath, indexName));
        }

        public bool Exists(string indexName)
        {
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
            using (var directory = FSDirectory.Open(new DirectoryInfo(Path.Combine(_rootPath, indexName))))
            {
                //using (var iwriter = new IndexWriter(directory, new IndexWriterConfig(Net.Util.LuceneVersion.LUCENE_48, new StandardAnalyzer()))
                //{
                //    Documents.Document doc = new Documents.Document();
                //    doc.Add(NewTextField("fieldname", text, Field.Store.YES));
                //    iwriter.AddDocument(doc);
                //}
            }
        }
    }
}
