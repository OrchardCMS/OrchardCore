using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms;

public class FormShapeTableProvider : IShapeTableProvider
{
    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe("Widget__Form")
            .OnDisplaying(context =>
            {
                if (context.Shape.TryGetProperty<ContentItem>("ContentItem", out var contentItem) && contentItem.ContentType == "Form")
                {
                    context.Shape.Metadata.Alternates.Add($"Widget__Form_{contentItem.ContentItemId.EncodeAlternateElement()}");

                    if (contentItem.TryGet<FormElementPart>(out var elementPart) && !string.IsNullOrEmpty(elementPart.Id))
                    {
                        context.Shape.Metadata.Alternates.Add($"Widget__Form_{elementPart.Id.EncodeAlternateElement()}");
                    }
                }
            });
    }
}
