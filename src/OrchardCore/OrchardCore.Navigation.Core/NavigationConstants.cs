namespace OrchardCore.Navigation;

public static class NavigationConstants
{
    public const string CssClassPrefix = "icon-class-";

    public const string AdminMenuContentPosition = "1";

    public const string AdminMenuDesignPosition = "2";

    public const string AdminMenuSearchPosition = "6";

    public const string AdminMenuSecurityPosition = "7";

    public const string AdminMenuAuditTrailPosition = "7.5";

    public const string AdminMenuWorkflowsPosition = "8";

    [Obsolete("This property is not longer used and will be removed. Instead, use AdminMenuToolsPosition")]
    public const string AdminMenuConfigurationPosition = "100";

    public const string AdminMenuToolsPosition = "100";

    public const string AdminMenuSettingsPosition = "after";

    public const string AdminId = "admin";

    public const string SiteSettingsId = "adminSettings";

    public const string AdminMenuId = "adminMenu";
}
