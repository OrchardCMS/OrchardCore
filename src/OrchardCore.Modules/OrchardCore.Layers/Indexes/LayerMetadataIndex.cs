using OrchardCore.ContentManagement;
using OrchardCore.Layers.Models;
using YesSql.Indexes;

namespace OrchardCore.Layers.Indexes
{
    public class LayerMetadataIndex : MapIndex
    {
        public string Zone { get; set; }
    }

    public class LayerMetadataIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<LayerMetadataIndex>()
                // Keep index records of soft deleted items as they are contained items.
                .When(contentItem => contentItem.Has<LayerMetadata>() && (contentItem.Published || contentItem.Latest))
                .Map(contentItem =>
                {
                    var layerMetadata = contentItem.As<LayerMetadata>();
                    if (layerMetadata == null)
                    {
                        return null;
                    }

                    return new LayerMetadataIndex
                    {
                        Zone = layerMetadata.Zone,
                    };
                });
        }
    }
}
