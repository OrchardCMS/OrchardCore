using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public static class ContentLocalizationPermissions
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", LocalizedString.Create("Localize content for others", typeof(ContentLocalizationPermissions)));

    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", LocalizedString.Create("Localize own content", typeof(ContentLocalizationPermissions)), new[] { LocalizeContent });

    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", LocalizedString.Create("Manage ContentCulturePicker settings", typeof(ContentLocalizationPermissions)));
}
