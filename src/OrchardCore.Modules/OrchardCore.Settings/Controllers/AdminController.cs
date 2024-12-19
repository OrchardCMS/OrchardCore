using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Controllers;

public sealed class AdminController : Controller
{
    private readonly IDisplayManager<ISite> _siteSettingsDisplayManager;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly CultureOptions _cultureOptions;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IShellReleaseManager shellReleaseManager,
        ISiteService siteService,
        IDisplayManager<ISite> siteSettingsDisplayManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IOptions<CultureOptions> cultureOptions,
        IUpdateModelAccessor updateModelAccessor,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _siteSettingsDisplayManager = siteSettingsDisplayManager;
        _shellReleaseManager = shellReleaseManager;
        _siteService = siteService;
        _notifier = notifier;
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _cultureOptions = cultureOptions.Value;
        H = htmlLocalizer;
    }

    [Admin("Settings/{groupId}", "AdminSettings")]
    public async Task<IActionResult> Index(string groupId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
        {
            return Forbid();
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var viewModel = new AdminIndexViewModel
        {
            GroupId = groupId,
            Shape = await _siteSettingsDisplayManager.BuildEditorAsync(site, _updateModelAccessor.ModelUpdater, false, groupId, string.Empty)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(string groupId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageGroupSettings, (object)groupId))
        {
            return Forbid();
        }

        var site = await _siteService.LoadSiteSettingsAsync();

        var viewModel = new AdminIndexViewModel
        {
            GroupId = groupId,
            Shape = await _siteSettingsDisplayManager.UpdateEditorAsync(site, _updateModelAccessor.ModelUpdater, false, groupId, string.Empty)
        };

        if (ModelState.IsValid)
        {
            await _siteService.UpdateSiteSettingsAsync(site);

            string culture = null;
            if (site.Properties.TryGetPropertyValue("LocalizationSettings", out var settings))
            {
                culture = settings.Value<string>("DefaultCulture");
            }

            // We create a transient scope with the newly selected culture to create a notification that will use it instead of the previous culture
            using (culture != null ? CultureScope.Create(culture, ignoreSystemSettings: _cultureOptions.IgnoreSystemSettings) : null)
            {
                await _notifier.SuccessAsync(H["Site settings updated successfully."]);
            }

            return RedirectToAction(nameof(Index), new { groupId });
        }
        else
        {
            // If the model state is invalid, suspend the request to release the shell so that the tenant is not reloaded.
            _shellReleaseManager.SuspendReleaseRequest();
        }

        return View(viewModel);
    }
}
