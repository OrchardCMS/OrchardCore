using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents.Services;

internal sealed class UserDisplayNameShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("UserDisplayName")
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                var displayType = shape.Metadata.DisplayType?.EncodeAlternateElement() ?? "Detail";

                if (shape.TryGetProperty<string>("Username", out var username))
                {
                    // UserDisplayName_[DisplayType]__[Username] e.g. UserDisplayName-johndoe.SummaryAdmin.cshtml
                    context.Shape.Metadata.Alternates.Add("UserDisplayName_" + displayType + "__" + username.EncodeAlternateElement());
                }

                // UserDisplayName_[DisplayType] e.g. UserDisplayName.SummaryAdmin.cshtml
                context.Shape.Metadata.Alternates.Add("UserDisplayName_" + displayType);
            })
            .OnProcessing(async context =>
            {
                var shape = context.Shape;

                var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                if (shape.TryGetProperty<string>("Username", out var username))
                {
                    var usernameShape = await shapeFactory.CreateAsync("UserDisplayNameText", Arguments.From(shape.Properties));

                    await shape.AddAsync(usernameShape, "5");
                }
            });

        return ValueTask.CompletedTask;
    }
}
