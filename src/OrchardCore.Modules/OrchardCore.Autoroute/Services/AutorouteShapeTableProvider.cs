using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Autoroute.Services;

public class AutorouteShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Content")
            .OnDisplaying(displaying => AddAlternates(displaying, "Content"));
        builder.Describe("Widget")
            .OnDisplaying(displaying => AddAlternates(displaying, "Widget"));

        return ValueTask.CompletedTask;
    }

    private static void AddAlternates(ShapeDisplayContext displaying, string shapeType)
    {
        var shape = displaying.Shape;
        var contentItem = shape.GetProperty<ContentItem>("ContentItem");

        var autoroutePart = contentItem?.As<AutoroutePart>();

        if (autoroutePart != null)
        {
            var encodedSlug = autoroutePart.Path.EncodeAlternateElement().Replace("/", "__");

            // shapeType__Slug__[Slug] e.g. Content-Slug-example, Widget-Slug-example
            displaying.Shape.Metadata.Alternates.Add($"{shapeType}__Slug__" + encodedSlug);

            // shapeType_[DisplayType]__Slug__[Slug] e.g. Content-Slug-example.Summary, Widget-Slug-example.Summary
            displaying.Shape.Metadata.Alternates.Add($"{shapeType}_" + displaying.Shape.Metadata.DisplayType + "__Slug__" + encodedSlug);
        }
    }
}
