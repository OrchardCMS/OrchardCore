using System;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Users;

public class UserMenuShapeTableProvider : IShapeTableProvider
{
    public const string ShapeTypePrefix = $"{ShapePrefix}__";

    private const string ShapePrefix = "UserMenuItems";

    private const string ShapeAlternatePrefix = $"{ShapePrefix}_";

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

                // UserMenuItems_{displayType} > UserMenuItems.{displayType}.cshtml.
                context.Shape.Metadata.Alternates.Add($"{ShapeAlternatePrefix}{context.Shape.Metadata.DisplayType}");

                // The value of 'subType' is the string that comes after 'UserMenuItems__'.
                var subType = context.Shape.Metadata.Type[ShapeTypePrefix.Length..].EncodeAlternateElement();

                // UserMenuItems_{displaType}__{subType} > UserMenuItems-{subType}.{displayType}.cshtml.
                context.Shape.Metadata.Alternates.Add($"{ShapeAlternatePrefix}{context.Shape.Metadata.DisplayType}__{subType}");
            });
    }
}
