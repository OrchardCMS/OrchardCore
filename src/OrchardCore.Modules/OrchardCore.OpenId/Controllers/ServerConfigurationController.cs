using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Controllers
{
    [Admin, Feature(OpenIdConstants.Features.Server)]
    public class ServerConfigurationController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IOpenIdServerService _serverService;
        private readonly IDisplayManager<OpenIdServerSettings> _serverSettingsDisplayManager;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHtmlLocalizer<ServerConfigurationController> H;

        public ServerConfigurationController(
            IAuthorizationService authorizationService,
            IHtmlLocalizer<ServerConfigurationController> htmlLocalizer,
            INotifier notifier,
            IOpenIdServerService serverService,
            IDisplayManager<OpenIdServerSettings> serverSettingsDisplayManager,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            H = htmlLocalizer;
            _notifier = notifier;
            _serverService = serverService;
            _serverSettingsDisplayManager = serverSettingsDisplayManager;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageServerSettings))
            {
                return Unauthorized();
            }

            var settings = await _serverService.GetSettingsAsync();
            var shape = await _serverSettingsDisplayManager.BuildEditorAsync(settings, updater: this, isNew: false);

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageServerSettings))
            {
                return Unauthorized();
            }

            var settings = await _serverService.GetSettingsAsync();
            var shape = await _serverSettingsDisplayManager.UpdateEditorAsync(settings, updater: this, isNew: false);

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            foreach (var result in await _serverService.ValidateSettingsAsync(settings))
            {
                if (result != ValidationResult.Success)
                {
                    var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                    ModelState.AddModelError(key, result.ErrorMessage);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            await _serverService.UpdateSettingsAsync(settings);

            _notifier.Success(H["OpenID server configuration successfully updated."]);

            await _shellHost.ReloadShellContextAsync(_shellSettings);

            return RedirectToAction(nameof(Index));
        }
    }
}
