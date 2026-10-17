using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public AdminSiteSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsUpdateNotifier optionsUpdateNotifier)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
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

        return Initialize<AdminSettingsViewModel, AdminSettings>("AdminSettings_Edit", static (model, settings) =>
        {
            model.DisplayThemeToggler = settings.DisplayThemeToggler;
            model.DisplayMenuFilter = settings.DisplayMenuFilter;
            model.DisplayQuickNavigation = settings.DisplayQuickNavigation;
            model.DisplayNewMenu = settings.DisplayNewMenu;
            model.DisplayTitlesInTopbar = settings.DisplayTitlesInTopbar;
            model.ShowBreadcrumb = settings.ShowBreadcrumb;
            model.MenuBehavior = settings.MenuBehavior;
            model.ListLayout = settings.ListLayout;
            model.ListActionsLayout = settings.ListActionsLayout;
            model.AllowUserListLayoutSelection = settings.AllowUserListLayoutSelection;
        }, settings).Location("Content:3")
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

        if (!context.Updater.ModelState.IsValid)
        {
            return await EditAsync(site, settings, context);
        }

        settings.DisplayThemeToggler = model.DisplayThemeToggler;
        settings.DisplayMenuFilter = model.DisplayMenuFilter;
        settings.DisplayQuickNavigation = model.DisplayQuickNavigation;
        settings.DisplayNewMenu = model.DisplayNewMenu;
        settings.DisplayTitlesInTopbar = model.DisplayTitlesInTopbar;
        settings.ShowBreadcrumb = model.ShowBreadcrumb;
        settings.MenuBehavior = model.MenuBehavior;

        var listLayout = model.ListLayout;
        var listActionsLayout = model.ListActionsLayout;

        if (settings.ListLayout != listLayout ||
            settings.ListActionsLayout != listActionsLayout ||
            settings.AllowUserListLayoutSelection != model.AllowUserListLayoutSelection)
        {
            // Refreshes IOptionsMonitor<AdminListOptions> once this shell scope commits.
            _optionsUpdateNotifier.RequestUpdate<AdminListOptions>();
        }

        settings.ListLayout = listLayout;
        settings.ListActionsLayout = listActionsLayout;
        settings.AllowUserListLayoutSelection = model.AllowUserListLayoutSelection;

        return await EditAsync(site, settings, context);
    }
}
