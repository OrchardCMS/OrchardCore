using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public static class FeaturesPermissions
{
    public static readonly Permission ManageFeatures = new("ManageFeatures", "Manage Features");
}
