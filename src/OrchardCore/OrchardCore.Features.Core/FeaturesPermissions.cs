using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Features;

public static class FeaturesPermissions
{
    public static readonly Permission ManageFeatures = new("ManageFeatures", LocalizedString.Create("Manage Features", typeof(FeaturesPermissions)));
}
