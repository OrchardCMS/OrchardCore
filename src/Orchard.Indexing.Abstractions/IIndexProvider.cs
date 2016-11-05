using System.Collections.Generic;

namespace Orchard.Indexing
{
    public interface IIndexProvider
    {
        /// <summary>
        /// Creates a new index
        /// </summary>
        void CreateIndex(string indexName);

        /// <summary>
        /// Checks whether an index is already existing or not
        /// </summary>
        bool Exists(string indexName);

        /// <summary>
        /// Lists all existing indexes
        /// </summary>
        IEnumerable<string> List();

        /// <summary>
        /// Deletes an existing index
        /// </summary>
        void DeleteIndex(string indexName);
        
        /// <summary>
        /// Adds a set of new document to the index
        /// </summary>
        void StoreDocuments(string indexName, IEnumerable<DocumentIndex> indexDocuments);
        
        /// <summary>
        /// Removes a set of existing document from the index
        /// </summary>
        void DeleteDocuments(string indexName, IEnumerable<int> documentIds);

        /// <summary>
        /// Gets the last index document id that was indexed for the current node.
        /// </summary>
        /// <remarks>
        /// Each instance decides how to store this value. For instance in Lucene it's store on the filesystem, so it's
        /// different for each instance, but it would be in the database for Projection. Each index can also be reseted,
        /// so the cursor needs to be different.
        /// </remarks>
        int GetLastIndexDocumentId(string indexName);
    }
}
