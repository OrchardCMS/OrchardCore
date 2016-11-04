using System.Collections.Generic;

namespace Orchard.Indexing
{
    public interface IIndexProvider
    {
        /// <summary>
        /// Creates a new index
        /// </summary>
        void CreateIndex(string name);

        /// <summary>
        /// Checks whether an index is already existing or not
        /// </summary>
        bool Exists(string name);

        /// <summary>
        /// Lists all existing indexes
        /// </summary>
        IEnumerable<string> List();

        /// <summary>
        /// Deletes an existing index
        /// </summary>
        void DeleteIndex(string name);
        
        /// <summary>
        /// Adds a set of new document to the index
        /// </summary>
        void Store(string indexName, IEnumerable<DocumentIndex> indexDocuments);
        
        /// <summary>
        /// Removes a set of existing document from the index
        /// </summary>
        void Delete(string indexName, IEnumerable<int> documentIds);
    }
}
