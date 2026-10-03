using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public static class ContentLocalizationPermissions
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", new LocalizationSource("Localize content for others", typeof(ContentLocalizationPermissions)));

    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", new LocalizationSource("Localize own content", typeof(ContentLocalizationPermissions)), new[] { LocalizeContent });

    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", new LocalizationSource("Manage ContentCulturePicker settings", typeof(ContentLocalizationPermissions)));
}
