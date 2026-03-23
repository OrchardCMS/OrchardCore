using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Alias.Services;

public sealed class WidgetAliasShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Widget")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                var aliasPart = contentItem?.As<AliasPart>();

                if (aliasPart != null)
                {
                    var displayType = displaying.Shape.Metadata.DisplayType;

                    // Get cached alternates and add them efficiently
                    var cachedAlternates = WidgetAliasAlternatesFactory.GetAlternates(
                        aliasPart.Alias,
                        displayType);

                    displaying.Shape.Metadata.Alternates.AddRange(cachedAlternates);
                }
            });

        return ValueTask.CompletedTask;
    }
}
