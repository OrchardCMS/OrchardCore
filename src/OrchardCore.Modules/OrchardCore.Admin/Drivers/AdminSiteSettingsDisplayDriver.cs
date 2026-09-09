using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Drivers;

public sealed class AdminSiteSettingsDisplayDriver : SiteDisplayDriver<AdminSettings>
{
    public const string GroupId = "admin";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptionsMonitor<AdminListOptions> _adminListOptions;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public AdminSiteSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsMonitor<AdminListOptions> adminListOptions,
        IOptionsUpdateNotifier optionsUpdateNotifier)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _adminListOptions = adminListOptions;
        _optionsUpdateNotifier = optionsUpdateNotifier;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, AdminSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, AdminPermissions.ManageAdminSettings))
        {
            return null;
        }

        var adminListOptions = _adminListOptions.CurrentValue;

        return Initialize<AdminSettingsViewModel, AdminSettings, AdminListOptions>("AdminSettings_Edit", static (model, settings, adminListOptions) =>
        {
            model.DisplayThemeToggler = settings.DisplayThemeToggler;
            model.DisplayMenuFilter = settings.DisplayMenuFilter;
            model.DisplayNewMenu = settings.DisplayNewMenu;
            model.DisplayTitlesInTopbar = settings.DisplayTitlesInTopbar;
            model.ListLayout = string.IsNullOrWhiteSpace(settings.ListLayout)
                ? adminListOptions.DefaultLayout
                : settings.ListLayout;
            model.ListActionsLayout = string.IsNullOrWhiteSpace(settings.ListActionsLayout)
                ? adminListOptions.DefaultActionsLayout
                : settings.ListActionsLayout;
        }, settings, adminListOptions).Location("Content:3")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AdminSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, AdminPermissions.ManageAdminSettings))
        {
            return null;
        }

        var model = new AdminSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.DisplayThemeToggler = model.DisplayThemeToggler;
        settings.DisplayMenuFilter = model.DisplayMenuFilter;
        settings.DisplayNewMenu = model.DisplayNewMenu;
        settings.DisplayTitlesInTopbar = model.DisplayTitlesInTopbar;
        var listLayout = string.IsNullOrWhiteSpace(model.ListLayout) ? null : model.ListLayout.Trim();
        var listActionsLayout = string.IsNullOrWhiteSpace(model.ListActionsLayout) ? null : model.ListActionsLayout.Trim();

        if (settings.ListLayout != listLayout || settings.ListActionsLayout != listActionsLayout)
        {
            // Refreshes IOptionsMonitor<AdminListOptions> once this shell scope commits.
            _optionsUpdateNotifier.RequestUpdate<AdminListOptions>();
        }

        settings.ListLayout = listLayout;
        settings.ListActionsLayout = listActionsLayout;

        return await EditAsync(site, settings, context);
    }
}
