using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Autoroute.Services;

public sealed class ContentAutorouteShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Content")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                if (contentItem is not null && contentItem.TryGet<AutoroutePart>(out var autoroutePart))
                {
                    var displayType = displaying.Shape.Metadata.DisplayType;

                    // Get cached alternates and add them efficiently
                    var cachedAlternates = AutorouteAlternatesFactory.GetAlternates(
                        autoroutePart.Path,
                        displayType);

                    displaying.Shape.Metadata.Alternates.AddRange(cachedAlternates);
                }
            });

        return ValueTask.CompletedTask;
    }
}
