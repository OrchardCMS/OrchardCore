using OrchardCore.ContentManagement;
using OrchardCore.Layers.Models;
using YesSql.Indexes;

namespace OrchardCore.Layers.Indexes
{
    public class AdminLayerMetadataIndex : MapIndex
    {
        public string Zone { get; set; }
    }

    public class AdminLayerMetadataIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AdminLayerMetadataIndex>()
            // TODO probably seperate.
                .When(contentItem => contentItem.Has<AdminLayerMetadata>())
                .Map(contentItem =>
                {
                    var layerMetadata = contentItem.As<AdminLayerMetadata>();
                    if (layerMetadata == null)
                    {
                        return null;
                    }

                    return new AdminLayerMetadataIndex
                    {
                        Zone = layerMetadata.Zone,
                    };
                });
        }
    }
}
