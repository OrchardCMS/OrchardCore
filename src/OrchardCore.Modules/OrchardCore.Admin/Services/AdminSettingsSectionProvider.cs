using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Services;

internal sealed class AdminSettingsSectionProvider : SiteSettingsSectionProvider<AdminSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public AdminSettingsSectionProvider(IOptionsUpdateNotifier optionsUpdateNotifier, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "admin",
        DisplayName = "Admin",
        FeatureId = "OrchardCore.Admin",
        Description = "Tenant admin interface settings. Omitted fields retain their values; a null layout restores the default one.",
    };

    protected override Permission ManagePermission => AdminPermissions.ManageAdminSettings;

    protected override void Describe(SiteSettingsSectionProperties<AdminSettings> properties)
    {
        properties.Boolean("displayThemeToggler", "Whether the theme toggler is displayed.", settings => settings.DisplayThemeToggler, (settings, value) => settings.DisplayThemeToggler = value);
        properties.Boolean("displayMenuFilter", "Whether the menu filter is displayed.", settings => settings.DisplayMenuFilter, (settings, value) => settings.DisplayMenuFilter = value);
        properties.Boolean("displayQuickNavigation", "Whether the quick navigation palette is displayed.", settings => settings.DisplayQuickNavigation, (settings, value) => settings.DisplayQuickNavigation = value);
        properties.Boolean("displayNewMenu", "Whether the New menu is displayed.", settings => settings.DisplayNewMenu, (settings, value) => settings.DisplayNewMenu = value);
        properties.Boolean("displayTitlesInTopbar", "Whether page titles are displayed in the top bar.", settings => settings.DisplayTitlesInTopbar, (settings, value) => settings.DisplayTitlesInTopbar = value);
        properties.Boolean("showBreadcrumb", "Whether the breadcrumb is displayed.", settings => settings.ShowBreadcrumb, (settings, value) => settings.ShowBreadcrumb = value);
        properties.Enum("menuBehavior", "How the sections of the admin menu open and close.", settings => settings.MenuBehavior, (settings, value) => settings.MenuBehavior = value);
        properties.NullableEnum("listLayout", "The layout of the admin lists, or null for the default one.", settings => settings.ListLayout, (settings, value) => settings.ListLayout = value);
        properties.NullableEnum("actionsLayout", "The layout of the admin actions, or null for the default one.", settings => settings.ActionsLayout, (settings, value) => settings.ActionsLayout = value);
        properties.Boolean("allowUserListLayoutSelection", "Whether a list offers users to pick another layout.", settings => settings.AllowUserListLayoutSelection, (settings, value) => settings.AllowUserListLayoutSelection = value);
    }

    protected override void OnChanging(ISite site, AdminSettings current, AdminSettings proposed)
    {
        if (current.ListLayout != proposed.ListLayout ||
            current.ActionsLayout != proposed.ActionsLayout ||
            current.AllowUserListLayoutSelection != proposed.AllowUserListLayoutSelection)
        {
            // Refreshes IOptionsMonitor<AdminListOptions> once this shell scope commits.
            _optionsUpdateNotifier.RequestUpdate<AdminListOptions>();
        }
    }
}
