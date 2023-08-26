using System;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Users;

public class UserMenuShapeTableProvider : IShapeTableProvider
{
    private const string _shapePrefix = "UserMenuItems";

    public void Discover(ShapeTableBuilder builder)
    {
        builder.Describe(_shapePrefix + "__*")
            .OnDisplaying(context =>
            {
                if (String.IsNullOrEmpty(context.Shape.Metadata.DisplayType) || context.Shape.Metadata.DisplayType == "Detail")
                {
                    return;
                }

                context.Shape.Metadata.Alternates.Add(_shapePrefix + "_" + context.Shape.Metadata.DisplayType);

                // SubType is the value after 'UserMenuItems__'.
                var subType = context.Shape.Metadata.Type[(_shapePrefix.Length + 2)..];

                // UserMenuItems_{displaType}__{part}.
                context.Shape.Metadata.Alternates.Add(_shapePrefix + "_" + context.Shape.Metadata.DisplayType + "__" + subType);
            });
    }
}
