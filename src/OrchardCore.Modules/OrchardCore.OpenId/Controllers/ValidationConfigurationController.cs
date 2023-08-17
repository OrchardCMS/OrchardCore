using System;
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
    [Admin, Feature(OpenIdConstants.Features.Validation)]
    public class ValidationConfigurationController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IOpenIdValidationService _validationService;
        private readonly IDisplayManager<OpenIdValidationSettings> _validationSettingsDisplayManager;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IHtmlLocalizer H;

        public ValidationConfigurationController(
            IAuthorizationService authorizationService,
            IHtmlLocalizer<ValidationConfigurationController> htmlLocalizer,
            INotifier notifier,
            IOpenIdValidationService validationService,
            IDisplayManager<OpenIdValidationSettings> validationSettingsDisplayManager,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IUpdateModelAccessor updateModelAccessor)
        {
            _authorizationService = authorizationService;
            H = htmlLocalizer;
            _notifier = notifier;
            _validationService = validationService;
            _validationSettingsDisplayManager = validationSettingsDisplayManager;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageValidationSettings))
            {
                return Forbid();
            }

            var settings = await _validationService.GetSettingsAsync();
            var shape = await _validationSettingsDisplayManager.BuildEditorAsync(settings, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");

            return View(shape);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageValidationSettings))
            {
                return Forbid();
            }

            var settings = await _validationService.GetSettingsAsync();
            var shape = await _validationSettingsDisplayManager.UpdateEditorAsync(settings, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            foreach (var result in await _validationService.ValidateSettingsAsync(settings))
            {
                if (result != ValidationResult.Success)
                {
                    var key = result.MemberNames.FirstOrDefault() ?? String.Empty;
                    ModelState.AddModelError(key, result.ErrorMessage);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(shape);
            }

            await _validationService.UpdateSettingsAsync(settings);

            await _notifier.SuccessAsync(H["OpenID validation configuration successfully updated."]);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            return RedirectToAction(nameof(Index));
        }
    }
}
