using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users;

public sealed class AdminDashboardShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("AdminDashboardContent")
            .OnDisplaying(async displaying =>
            {
                await displaying.Shape.AddAsync(new ShapeViewModel("AdminDashboardUsers"), "30");
            });

        return ValueTask.CompletedTask;
    }
}
