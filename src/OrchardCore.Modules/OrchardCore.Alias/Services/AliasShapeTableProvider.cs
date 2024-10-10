using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Alias.Services;

public class AliasShapeTableProvider : ShapeTableProvider
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

        var aliasPart = contentItem?.As<AliasPart>();

        if (aliasPart != null)
        {
            var encodedAlias = aliasPart.Alias.EncodeAlternateElement();

            // shapeType__Alias__[Alias] e.g. Content-Alias-example, Widget-Alias-example
            displaying.Shape.Metadata.Alternates.Add($"{shapeType}__Alias__" + encodedAlias);

            // shapeType_[DisplayType]__Alias__[Alias] e.g. Content-Alias-example.Summary, Widget-Alias-example.Summary
            displaying.Shape.Metadata.Alternates.Add($"{shapeType}_" + displaying.Shape.Metadata.DisplayType + "__Alias__" + encodedAlias);
        }
    }
}
