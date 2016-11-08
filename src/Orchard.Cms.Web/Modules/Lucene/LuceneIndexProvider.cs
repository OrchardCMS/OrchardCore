using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;
using Orchard.Indexing;

namespace Lucene
{
    public class LuceneIndexProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _rootPath;

        public LuceneIndexProvider(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _hostingEnvironment = hostingEnvironment;
            _rootPath = Path.Combine(shellOptions.Value.ShellsRootContainerName, shellOptions.Value.ShellsContainerName, shellSettings.Name, "Lucene");
            
            Directory.CreateDirectory(_rootPath);
        }

        public void CreateIndex(string indexName)
        {
            using (File.CreateText(Path.Combine(_rootPath, indexName + ".txt"))) { }
        }

        public void DeleteDocuments(string indexName, IEnumerable<int> documentIds)
        {
        }

        public void DeleteIndex(string indexName)
        {
            File.Delete(Path.Combine(_rootPath, indexName + ".txt"));
        }

        public bool Exists(string indexName)
        {
            return File.Exists(Path.Combine(_rootPath, indexName + ".txt"));
        }

        public int GetLastIndexDocumentId(string indexName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> List()
        {
            return _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(_rootPath).Where(x => x.Name.EndsWith(".txt")).Select(x => Path.GetFileNameWithoutExtension(x.Name));
        }

        public void StoreDocuments(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            File.AppendAllLines(Path.Combine(_rootPath, indexName + ".txt"), indexDocuments.Select(x => x.DocumentId.ToString()));
        }
    }
}
