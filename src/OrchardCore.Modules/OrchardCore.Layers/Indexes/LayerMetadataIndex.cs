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
                .When(contentItem => contentItem.Has<LayerMetadata>())
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

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
