using Orchard.ContentManagement;
using Orchard.Layers.Models;
using YesSql.Core.Indexes;

namespace Orchard.Layers.Indexes
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
                .Map(contentItem =>
                {
                    var layerMetadata = contentItem.As<LayerMetadata>();
                    if(layerMetadata != null)
                    {
                        return new LayerMetadataIndex
						{
                            Zone = layerMetadata.Zone,
                        };
                    }

                    return null;
                });
        }
    }
}