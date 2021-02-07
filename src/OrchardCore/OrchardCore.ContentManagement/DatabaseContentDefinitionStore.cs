using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Documents;

namespace OrchardCore.ContentManagement
{
    public class DatabaseContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IDocumentManager<ContentDefinitionRecord> _documentManager;

        public DatabaseContentDefinitionStore(IDocumentManager<ContentDefinitionRecord> documentManager)
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
