using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Indexing;

namespace Lucene
{
    public class LuceneIndexProvider : IIndexProvider
    {
        public void CreateIndex(string name)
        {
            throw new NotImplementedException();
        }

        public void Delete(string indexName, IEnumerable<int> documentIds)
        {
            throw new NotImplementedException();
        }

        public void DeleteIndex(string name)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> List()
        {
            throw new NotImplementedException();
        }

        public void Store(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            throw new NotImplementedException();
        }
    }
}
