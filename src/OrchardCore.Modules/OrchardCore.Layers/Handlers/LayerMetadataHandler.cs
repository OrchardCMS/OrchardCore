using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Handlers
{
    public class LayerMetadataHandler : ContentHandlerBase
    {
        private readonly IVolatileDocumentManager<LayerState> _layerStateManager;

        public LayerMetadataHandler(IVolatileDocumentManager<LayerState> layerStateManager)
        {
            _layerStateManager = layerStateManager;
        }

        public override Task PublishedAsync(PublishContentContext context) => UpdateAsync(context.ContentItem);

        public override Task RemovedAsync(RemoveContentContext context) => UpdateAsync(context.ContentItem);

        public override Task UnpublishedAsync(PublishContentContext context) => UpdateAsync(context.ContentItem);

        private Task UpdateAsync(ContentItem contentItem)
        {
            var layerMetadata = contentItem.As<LayerMetadata>();

            if (layerMetadata == null)
            {
                return Task.CompletedTask;
            }

            // Checked by the 'LayerFilter'.
            return _layerStateManager.UpdateAsync(new LayerState());
        }
    }

    public class LayerState : Document
    {
    }
}
