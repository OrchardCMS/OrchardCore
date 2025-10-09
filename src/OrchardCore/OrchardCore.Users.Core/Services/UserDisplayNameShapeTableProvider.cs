using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Users.Core.Services;

public sealed class UserDisplayNameShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("UserDisplayName")
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                var displayType = shape.Metadata.DisplayType?.EncodeAlternateElement() ?? "Detail";

                if (shape.TryGetProperty<string>("UserName", out var username))
                {
                    // UserDisplayName_[DisplayType]__[UserName] e.g. UserDisplayName-johndoe.SummaryAdmin.cshtml
                    shape.Metadata.Alternates.Add("UserDisplayName_" + displayType + "__" + username.EncodeAlternateElement());
                }

                // UserDisplayName_[DisplayType] e.g. UserDisplayName.SummaryAdmin.cshtml
                shape.Metadata.Alternates.Add("UserDisplayName_" + displayType);
            });

        return ValueTask.CompletedTask;
    }
}
