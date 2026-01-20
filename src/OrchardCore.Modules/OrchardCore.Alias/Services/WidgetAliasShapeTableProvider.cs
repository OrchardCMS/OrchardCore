using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

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
                    var encodedAlias = aliasPart.Alias.EncodeAlternateElement();

                    // Widget__Alias__[Alias] e.g. Widget-Alias-example, Widget-Alias-my-page
                    displaying.Shape.Metadata.Alternates.Add("Widget__Alias__" + encodedAlias);

                    // Widget_[DisplayType]__Alias__[Alias] e.g. Widget-Alias-example.Summary, Widget-Alias-my-page.Summary
                    displaying.Shape.Metadata.Alternates.Add("Widget_" + displaying.Shape.Metadata.DisplayType + "__Alias__" + encodedAlias);
                }
            });

        return ValueTask.CompletedTask;
    }
}
