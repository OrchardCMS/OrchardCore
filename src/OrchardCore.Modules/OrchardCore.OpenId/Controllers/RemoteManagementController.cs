using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Modules;

namespace OrchardCore.OpenId.Controllers;

[Admin, Feature("OrchardCore.OpenId.RemoteManagement")]
public sealed class RemoteManagementController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly RemoteManagementConfigurationService _configurationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    internal readonly IHtmlLocalizer H;

    public RemoteManagementController(
        IAuthorizationService authorizationService,
        INotifier notifier,
        RemoteManagementConfigurationService configurationService,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHtmlLocalizer<RemoteManagementController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        _configurationService = configurationService;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        H = htmlLocalizer;
    }

    [Admin("RemoteManagement", "RemoteManagement")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration))
        {
            return Forbid();
        }

        return View(await _configurationService.GetStatusAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Configure()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration))
        {
            return Forbid();
        }

        await _configurationService.ConfigureAsync();
        await _notifier.SuccessAsync(H["Remote Management authentication was configured successfully."]);
        await _shellHost.ReleaseShellContextAsync(_shellSettings);

        return RedirectToAction(nameof(Index));
    }
}
