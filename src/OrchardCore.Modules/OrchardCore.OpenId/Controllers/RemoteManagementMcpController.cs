using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId.Controllers;

[Admin, Feature("OrchardCore.OpenId.RemoteManagement.Mcp")]
public sealed class RemoteManagementMcpController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly RemoteManagementMcpConfigurationService _configurationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly INotifier _notifier;
    internal readonly IHtmlLocalizer H;

    public RemoteManagementMcpController(IAuthorizationService authorizationService,
        RemoteManagementMcpConfigurationService configurationService,
        IShellHost shellHost, ShellSettings shellSettings, INotifier notifier,
        IHtmlLocalizer<RemoteManagementMcpController> localizer)
    {
        _authorizationService = authorizationService;
        _configurationService = configurationService;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _notifier = notifier;
        H = localizer;
    }

    [Admin("RemoteManagement/Mcp", "RemoteManagementMcp")]
    public async Task<IActionResult> Index(string clientId = "orchardcore-mcp")
    {
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration))
        {
            return Forbid();
        }

        return View(await _configurationService.GetConfigurationAsync(clientId));
    }

    /// <summary>Configures shared authentication without pre-registering a particular client.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureAutomatic([FromServices] IRemoteManagementTenantConfigurationService configuration)
    {
        // Configuring rewrites the OpenID server settings, which managing remote management doesn't imply.
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration) ||
            !await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageServerSettings))
        {
            return Forbid();
        }

        try
        {
            await configuration.ConfigureAsync();
        }
        catch (ValidationException exception)
        {
            await _notifier.ErrorAsync(H["Remote Management could not be configured: {0}", exception.Message]);
            return RedirectToAction(nameof(Index));
        }

        await _notifier.SuccessAsync(H["MCP authentication is configured. Compatible clients can register automatically when connecting."]);
        await _shellHost.ReleaseShellContextAsync(_shellSettings);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Configure(RemoteManagementMcpViewModel model)
    {
        // Configuring rewrites the OpenID server settings, which managing remote management doesn't imply.
        if (!await _authorizationService.AuthorizeAsync(User, RemoteManagementPermissions.ManageRemoteManagementConfiguration) ||
            !await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageServerSettings))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _configurationService.ConfigureAsync(model);
            }
            catch (ValidationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            model.IsConfigured = false;
            return View(nameof(Index), model);
        }

        await _notifier.SuccessAsync(H["MCP client authentication was configured successfully."]);
        await _shellHost.ReleaseShellContextAsync(_shellSettings);
        return RedirectToAction(nameof(Index), new { model.ClientId });
    }
}
