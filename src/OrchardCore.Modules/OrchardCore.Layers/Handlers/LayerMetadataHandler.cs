using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Cache;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Handlers
{
    public class LayerMetadataHandler : ContentHandlerBase
    {
        public const string LayerChangeToken = "OrchardCore.Layers:LayerMetadata";

        private readonly ISignal _signal;

        public LayerMetadataHandler(ISignal signal)
        {
            _signal = signal;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            SignalLayerChanged(context.ContentItem);
            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            SignalLayerChanged(context.ContentItem);
            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            SignalLayerChanged(context.ContentItem);
            return Task.CompletedTask;
        }

        private void SignalLayerChanged(ContentItem contentItem)
        {
            var layerMetadata = contentItem.As<LayerMetadata>();

            if (layerMetadata != null)
            {
                _signal.SignalToken(LayerChangeToken);
            }
        }
    }
}