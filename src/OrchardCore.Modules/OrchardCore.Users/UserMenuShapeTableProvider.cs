using System;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Users;

public class UserMenuShapeTableProvider : IShapeTableProvider
{
    private const string BaseShapeType = "UserMenuItems";

    private const string ShapeTypePrefix = $"{BaseShapeType}__";

    private const string ShapeAlternatePrefix = $"{BaseShapeType}_";

    public void Discover(ShapeTableBuilder builder)
    {
        // Describe any shape-type that starts with 'UserMenuItems__'.
        builder.Describe($"{ShapeTypePrefix}*")
            .OnDisplaying(context =>
            {
                if (String.IsNullOrEmpty(context.Shape.Metadata.DisplayType) || context.Shape.Metadata.DisplayType == "Detail")
                {
                    return;
                }

                // 'UserMenuItems_{displayType}' e.g. 'UserMenuItems.DetailAdmin.cshtml'.
                context.Shape.Metadata.Alternates.Add($"{ShapeAlternatePrefix}{context.Shape.Metadata.DisplayType}");

                // The value of 'subType' is the encoded string that comes after 'UserMenuItems__'.
                var subType = context.Shape.Metadata.Type[ShapeTypePrefix.Length..].EncodeAlternateElement();

                // 'UserMenuItems_{displaType}__{subType}' e.g. 'UserMenuItems-Dashboard.DetailAdmin.cshtml'.
                context.Shape.Metadata.Alternates.Add($"{ShapeAlternatePrefix}{context.Shape.Metadata.DisplayType}__{subType}");
            });
    }
}
