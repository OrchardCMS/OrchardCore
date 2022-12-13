using System;
using System.Threading.Tasks;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A Singleton service used to persist documents to file storage.
    /// </summary>
    public interface IDocumentFileStore
    {
        /// <summary>
        /// Loads a single document of the specified <paramref name="documentType"/>.
        /// </summary>
        /// <param name="documentType">The type of document to retrieve</param>
        /// <returns>Read document or null if does not exists.</returns>
        Task<object> GetDocumentAsync(Type documentType);

        /// <summary>
        /// Saves a single document of the specified <paramref name="documentType"/>.
        /// </summary>
        /// <param name="documentType">The type of document to save</param>
        /// <param name="document">The document to save</param>
        Task SaveDocumentAsync(Type documentType, object document);
    }
}
