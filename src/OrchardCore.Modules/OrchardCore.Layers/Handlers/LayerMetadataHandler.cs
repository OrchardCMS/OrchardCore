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
        public const string StateKey = "OrchardCore.Layers:LayerMetadata";

        private readonly IVolatileStates _states;

        public LayerMetadataHandler(IVolatileStates states)
        {
            _states = states;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return UpdateLayerIdentifierAsync(context.ContentItem);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return UpdateLayerIdentifierAsync(context.ContentItem);
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            return UpdateLayerIdentifierAsync(context.ContentItem);
        }

        private Task UpdateLayerIdentifierAsync(ContentItem contentItem)
        {
            var layerMetadata = contentItem.As<LayerMetadata>();

            if (layerMetadata == null)
            {
                return Task.CompletedTask;
            }

            return _states.SetAsync(StateKey, new Document() { Identifier = IdGenerator.GenerateId() });
        }
    }
}
