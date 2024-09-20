using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization;

public static class ContentLocalizationPermissions
{
    public static readonly Permission LocalizeContent = new("LocalizeContent", "Localize content for others");

    public static readonly Permission LocalizeOwnContent = new("LocalizeOwnContent", "Localize own content", new[] { LocalizeContent });

    public static readonly Permission ManageContentCulturePicker = new("ManageContentCulturePicker", "Manage ContentCulturePicker settings");
}
