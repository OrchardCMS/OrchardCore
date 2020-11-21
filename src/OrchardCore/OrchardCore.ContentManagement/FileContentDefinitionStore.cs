using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;

namespace OrchardCore.ContentManagement
{
    public class FileContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IDocumentManager<IFileDocumentStore, ContentDefinitionRecord> _documentManager;

        public FileContentDefinitionStore(IDocumentManager<IFileDocumentStore, ContentDefinitionRecord> documentManager)
        {
            _documentManager = documentManager;
        }

        /// <inheritdoc />
        public Task<ContentDefinitionRecord> LoadContentDefinitionAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <inheritdoc />
        public Task<ContentDefinitionRecord> GetContentDefinitionAsync() => _documentManager.GetOrCreateImmutableAsync();

        /// <inheritdoc />
        public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord) => _documentManager.UpdateAsync(contentDefinitionRecord);
    }
}
