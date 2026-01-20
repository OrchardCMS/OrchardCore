using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Alias.Services;

public sealed class ContentAliasShapeTableProvider : ShapeTableProvider
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

                    // Content__Alias__[Alias] e.g. Content-Alias-example, Content-Alias-my-page
                    displaying.Shape.Metadata.Alternates.Add("Content__Alias__" + encodedAlias);

                    // Content_[DisplayType]__Alias__[Alias] e.g. Content-Alias-example.Summary, Content-Alias-my-page.Summary
                    displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__Alias__" + encodedAlias);
                }
            });

        return ValueTask.CompletedTask;
    }
}
