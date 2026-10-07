using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public static class FeaturesPermissions
{
    public static readonly Permission ManageFeatures = new("ManageFeatures", LocalizationSource.Create("Manage Features", typeof(FeaturesPermissions)));
}
