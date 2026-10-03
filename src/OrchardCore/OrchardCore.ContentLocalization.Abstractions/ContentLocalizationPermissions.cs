using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public static class ContentLocalizationPermissions
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", LocalizationSource.Create("Localize content for others", typeof(ContentLocalizationPermissions)));

    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", LocalizationSource.Create("Localize own content", typeof(ContentLocalizationPermissions)), new[] { LocalizeContent });

    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", LocalizationSource.Create("Manage ContentCulturePicker settings", typeof(ContentLocalizationPermissions)));
}
