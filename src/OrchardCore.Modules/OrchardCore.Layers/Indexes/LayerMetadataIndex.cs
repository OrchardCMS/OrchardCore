using OrchardCore.ContentManagement;
using OrchardCore.Layers.Models;
using YesSql.Indexes;

namespace OrchardCore.Layers.Indexes;

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
                if (!contentItem.TryGet<LayerMetadata>(out var layerMetadata))
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
