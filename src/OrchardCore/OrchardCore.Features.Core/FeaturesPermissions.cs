using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public sealed class FeaturesPermissions
{
    public static readonly Permission ManageFeatures = new("ManageFeatures", "Manage Features");
}
