using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Alias.Services;

public class AliasShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Content")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                var aliasPart = contentItem?.As<AliasPart>();

                if (aliasPart != null)
                {
                    var encodedAlias = aliasPart.Alias.EncodeAlternateElement();
                    displaying.Shape.Metadata.Alternates.Add("Content__" + encodedAlias);
                    
                    var encodedContentType = contentItem.ContentType.EncodeAlternateElement();
                    displaying.Shape.Metadata.Alternates.Add("Content__" + encodedContentType + "__" + encodedAlias);
                }
            });

        return ValueTask.CompletedTask;
    }
}
