using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Admin;

public sealed class AdminDashboardShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("AdminDashboardContent")
            .OnDisplaying(async displaying =>
            {
                await displaying.Shape.AddAsync(new ShapeViewModel("AdminDashboardDocumentation"), "40");
            });

        return ValueTask.CompletedTask;
    }
}
