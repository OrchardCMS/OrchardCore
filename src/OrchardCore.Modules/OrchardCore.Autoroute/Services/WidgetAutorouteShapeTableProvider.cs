using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Autoroute.Services;

public sealed class WidgetAutorouteShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Widget")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                var autoroutePart = contentItem?.As<AutoroutePart>();

                if (autoroutePart != null)
                {
                    var encodedSlug = autoroutePart.Path.EncodeAlternateElement().Replace("/", "__");

                    // Widget__Slug__[Slug] e.g. Widget-Slug-example, Widget-Slug-blog-my-post
                    displaying.Shape.Metadata.Alternates.Add("Widget__Slug__" + encodedSlug);

                    // Widget_[DisplayType]__Slug__[Slug] e.g. Widget-Slug-example.Summary, Widget-Slug-blog-my-post.Summary
                    displaying.Shape.Metadata.Alternates.Add("Widget_" + displaying.Shape.Metadata.DisplayType + "__Slug__" + encodedSlug);
                }
            });

        return ValueTask.CompletedTask;
    }
}
