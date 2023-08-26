using System;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Users;

public class UserMenuShapeTableProvider : IShapeTableProvider
{
    private const string _shapePrefix = "UserMenuItems";

    public void Discover(ShapeTableBuilder builder)
    {
        // Describe any shape-type that starts with 'UserMenuItems__'.
        builder.Describe(_shapePrefix + "__*")
            .OnDisplaying(context =>
            {
                if (String.IsNullOrEmpty(context.Shape.Metadata.DisplayType) || context.Shape.Metadata.DisplayType == "Detail")
                {
                    return;
                }

                // UserMenuItems_{displayType} > UserMenuItems.{displayType}.cshtml.
                context.Shape.Metadata.Alternates.Add(_shapePrefix + '_' + context.Shape.Metadata.DisplayType);

                // The value of 'subType' is the string that comes after 'UserMenuItems__'.
                var subType = context.Shape.Metadata.Type[(_shapePrefix.Length + 2)..];

                // UserMenuItems_{displaType}__{subType} > UserMenuItems-{subType}.{displayType}.cshtml.
                context.Shape.Metadata.Alternates.Add(_shapePrefix + '_' + context.Shape.Metadata.DisplayType + "__" + subType);
            });
    }
}
