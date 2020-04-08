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
        public const string LayerIdentifier = "OrchardCore.Layers:LayerMetadata";

        private readonly IVolatilePropertiesService _properties;

        public LayerMetadataHandler(IVolatilePropertiesService properties)
        {
            _properties = properties;
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

            return _properties.SetAsync(LayerIdentifier, new BaseDocument() { Identifier = IdGenerator.GenerateId() });
        }
    }
}
