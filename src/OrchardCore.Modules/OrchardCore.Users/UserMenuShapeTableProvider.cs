using System;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Users;

public class UserMenuShapeTableProvider : IShapeTableProvider
{
    private const string _shapePrefix = "UserMenuItems__";

    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe(_shapePrefix + "*")
            .OnDisplaying(context =>
            {
                if (String.IsNullOrEmpty(context.Shape.Metadata.DisplayType) || context.Shape.Metadata.DisplayType == "Detail")
                {
                    return;
                }

                context.Shape.Metadata.Alternates.Add("UserMenuItems_" + context.Shape.Metadata.DisplayType);

                // Part is the value after 'UserMenuItems__'.
                var part = context.Shape.Metadata.Type[(_shapePrefix.Length + 1)..];

                // UserMenu is 'UserMenuItems_'.
                var userMenu = _shapePrefix[..^1];

                // UserMenuItems_{displaType}__{part}.
                context.Shape.Metadata.Alternates.Add(userMenu + context.Shape.Metadata.DisplayType + "__" + part);
            });
    }
}
