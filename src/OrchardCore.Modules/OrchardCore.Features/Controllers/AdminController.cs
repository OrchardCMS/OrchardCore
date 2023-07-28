using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Services;
using OrchardCore.Features.ViewModels;
using OrchardCore.Routing;

namespace OrchardCore.Features.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly AdminOptions _adminOptions;
        private readonly INotifier _notifier;

        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;

        public AdminController(
            IAuthorizationService authorizationService,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IExtensionManager extensionManager,
            IShellFeaturesManager shellFeaturesManager,
            IOptions<AdminOptions> adminOptions,
            INotifier notifier,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _authorizationService = authorizationService;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _extensionManager = extensionManager;
            _shellFeaturesManager = shellFeaturesManager;
            _adminOptions = adminOptions.Value;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<ActionResult> Features(string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var viewModel = new FeaturesViewModel();

            await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
            {
                // If the user provide an invalid tenant value, we'll set it to null so it's not available on the next request.
                if (isProxy)
                {
                    viewModel.IsProxy = true;
                    tenant = settings.Name;
                    viewModel.Name = settings.Name;
                }

                viewModel.Features = await featureService.GetModuleFeaturesAsync();
            });

            return View(viewModel);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> Features(BulkActionViewModel model, bool? force, string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            if (model.FeatureIds == null || model.FeatureIds.Length == 0)
            {
                ModelState.AddModelError(nameof(BulkActionViewModel.FeatureIds), S["Please select one or more features."]);
            }

            if (ModelState.IsValid)
            {
                await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
                {
                    var availableFeatures = await featureService.GetAvailableFeatures(model.FeatureIds);

                    await featureService.EnableOrDisableFeaturesAsync(availableFeatures, model.BulkAction, force, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
                });
            }

            return RedirectToAction(nameof(Features));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id, string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var found = false;

            await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
            {
                var feature = await featureService.GetAvailableFeature(id);

                if (feature == null)
                {
                    return;
                }

                found = true;

                if (!isProxy && id == FeaturesConstants.FeatureId)
                {
                    await _notifier.ErrorAsync(H["This feature is always enabled and cannot be disabled."]);

                    return;
                }

                await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Disable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            });

            if (!found)
            {
                return NotFound();
            }

            return Redirect(GetNextUrl(tenant, id));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id, string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var found = false;

            await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
            {
                var feature = await featureService.GetAvailableFeature(id);

                if (feature == null)
                {
                    return;
                }

                found = true;

                await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Enable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            });

            if (!found)
            {
                return NotFound();
            }

            return Redirect(GetNextUrl(tenant, id));
        }

        private async Task ExecuteAsync(string tenant, Func<FeatureService, ShellSettings, bool, Task> action)
        {
            if (_shellSettings.IsDefaultShell() &&
                !String.IsNullOrWhiteSpace(tenant) &&
                _shellHost.TryGetSettings(tenant, out var settings) &&
                !settings.IsDefaultShell() &&
                settings.IsRunning())
            {
                // At this point, we know that this request is being executed from the Default tenant.
                // Also, we were able to find a matching and running tenant that isn't the Default one.
                // Therefore, it is safe to create a scope for the given tenant.
                var shellScope = await _shellHost.GetScopeAsync(settings);

                await shellScope.UsingAsync(async scope =>
                {
                    var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                    var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

                    // At this point we apply the action on the given tenant.
                    await action(new FeatureService(shellFeatureManager, extensionManager), scope.ShellContext.Settings, true);
                });

                return;
            }

            // At this point we apply the action on the current tenant.
            await action(new FeatureService(_shellFeaturesManager, _extensionManager), _shellSettings, false);
        }

        private string GetNextUrl(string tenant, string featureId)
        {
            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            ShellSettings settings = null;

            if (!String.IsNullOrWhiteSpace(tenant))
            {
                _shellHost.TryGetSettings(tenant, out settings);
            }

            if (settings != null && settings.Name != _shellSettings.Name)
            {
                return Url.Action(nameof(Features), new { tenant });
            }

            if (!settings.IsDefaultShell() && featureId == FeaturesConstants.FeatureId)
            {
                return Url.Content("~/" + _adminOptions.AdminUrlPrefix);
            }

            return Url.Action(nameof(Features));
        }

        private async ValueTask NotifyAsync(IEnumerable<IFeatureInfo> features, bool enabled = true)
        {
            foreach (var feature in features)
            {
                await _notifier.SuccessAsync(H["{0} was {1}.", feature.Name ?? feature.Id, enabled ? "enabled" : "disabled"]);
            }
        }
    }
}
