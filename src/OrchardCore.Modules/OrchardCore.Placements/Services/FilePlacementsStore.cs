using System.Threading.Tasks;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;
using OrchardCore.Placements.Models;

namespace OrchardCore.Placements.Services
{
    public class FilePlacementsStore : IPlacementStore
    {
        private readonly IDocumentManager<IFileDocumentStore, PlacementsDocument> _documentManager;

        public FilePlacementsStore(IDocumentManager<IFileDocumentStore, PlacementsDocument> documentManager)
        {
            _documentManager = documentManager;
        }

        /// <inheritdoc />
        public Task<PlacementsDocument> LoadPlacementsAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <inheritdoc />
        public Task<PlacementsDocument> GetPlacementsAsync() => _documentManager.GetOrCreateImmutableAsync();

        /// <inheritdoc />
        public Task SavePlacementsAsync(PlacementsDocument placementsDocument) => _documentManager.UpdateAsync(placementsDocument);
    }
}
