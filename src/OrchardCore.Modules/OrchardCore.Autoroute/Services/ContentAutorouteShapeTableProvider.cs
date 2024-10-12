using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

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

                var autoroutePart = contentItem?.As<AutoroutePart>();

                if (autoroutePart != null)
                {
                    var encodedSlug = autoroutePart.Path.EncodeAlternateElement().Replace("/", "__");

                    // Content__Slug__[Slug] e.g. Content-Slug-example, Content-Slug-blog-my-post
                    displaying.Shape.Metadata.Alternates.Add("Content__Slug__" + encodedSlug);

                    // Content_[DisplayType]__Slug__[Slug] e.g. Content-Slug-example.Summary, Content-Slug-blog-my-post.Summary
                    displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__Slug__" + encodedSlug);
                }
            });

        return ValueTask.CompletedTask;
    }
}
