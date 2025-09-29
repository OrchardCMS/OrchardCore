using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Users.Core.Services;

public sealed class UserDisplayNameShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("UserDisplayName")
            .OnProcessing(async context =>
            {
                var shape = context.Shape;

                var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                if (shape.TryGetProperty<string>("UserName", out var username))
                {
                    var usernameShape = await shapeFactory.CreateAsync("UserDisplayNameText", Arguments.From(shape.Properties));

                    await shape.AddAsync(usernameShape, "5");
                }
            });

        return ValueTask.CompletedTask;
    }
}
