using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId.Controllers;

[Admin, Feature("OrchardCore.OpenId.RemoteManagement.Cli")]
public sealed class RemoteManagementCliController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IRemoteManagementCliConfigurationService _cliConfiguration;
    private readonly IRemoteManagementTenantConfigurationService _tenantConfiguration;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly INotifier _notifier;
    internal readonly IHtmlLocalizer H;

    public RemoteManagementCliController(IAuthorizationService authorizationService,
        IRemoteManagementCliConfigurationService cliConfiguration,
        IRemoteManagementTenantConfigurationService tenantConfiguration,
        IShellHost shellHost, ShellSettings shellSettings, INotifier notifier,
        IHtmlLocalizer<RemoteManagementCliController> localizer)
    {
        _authorizationService = authorizationService;
        _cliConfiguration = cliConfiguration;
        _tenantConfiguration = tenantConfiguration;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _notifier = notifier;
        H = localizer;
    }

    [Admin("RemoteManagement/Cli", "RemoteManagementCli")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration))
        {
            return Forbid();
        }

        return View(await _cliConfiguration.IsConfiguredAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Configure()
    {
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration))
        {
            return Forbid();
        }

        await _tenantConfiguration.ConfigureAsync();
        await _cliConfiguration.ConfigureAsync();
        await _notifier.SuccessAsync(H["Pomi CLI authentication was configured successfully."]);
        await _shellHost.ReleaseShellContextAsync(_shellSettings);
        return RedirectToAction(nameof(Index));
    }
}
