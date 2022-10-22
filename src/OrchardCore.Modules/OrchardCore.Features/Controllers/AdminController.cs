using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
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
        private readonly INotifier _notifier;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            IShellHost shellHost,
            ShellSettings shellSettings,
            INotifier notifier,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _extensionManager = extensionManager;
            _shellFeaturesManager = shellFeaturesManager;
            H = localizer;
            S = stringLocalizer;
        }

        public async Task<ActionResult> Features(string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var viewModel = new FeaturesViewModel();

            await ExecuteAsync(tenant, async (featureService, settings) =>
            {
                // if the user provide an invalid tenant value, we'll set it to null so it's not available on the next request
                tenant = settings?.Name;
                viewModel.Name = settings?.Name;
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

            if (model.FeatureIds == null || !model.FeatureIds.Any())
            {
                ModelState.AddModelError(nameof(BulkActionViewModel.FeatureIds), S["Please select one or more features."]);
            }

            if (ModelState.IsValid)
            {
                await ExecuteAsync(tenant, async (featureService, settings) =>
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

            await ExecuteAsync(tenant, async (featureService, settings) =>
            {
                var feature = await featureService.GetAvailableFeature(id);

                if (feature != null)
                {
                    found = true;
                }

                await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Disable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            });

            if (!found)
            {
                return NotFound();
            }

            return Redirect(GetNextUrl(tenant));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id, string tenant)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var found = false;

            await ExecuteAsync(tenant, async (featureService, settings) =>
            {
                var feature = await featureService.GetAvailableFeature(id);

                if (feature != null)
                {
                    found = true;
                }

                await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Enable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            });

            if (!found)
            {
                return NotFound();
            }

            return Redirect(GetNextUrl(tenant));
        }

        private async Task ExecuteAsync(string tenant, Func<FeatureService, ShellSettings, Task> action)
        {
            if (_shellSettings.IsDefaultShell()
                && !String.IsNullOrWhiteSpace(tenant)
                && _shellHost.TryGetSettings(tenant, out var settings)
                && !settings.IsDefaultShell()
                && settings.State == TenantState.Running)
            {
                // At this point we know that this request is being executed from the host.
                // Also, we were able to find a matching running tenant that isn't a default shell
                // we are safe to create a scope for the given tenant
                var shellScope = await _shellHost.GetScopeAsync(settings);

                await shellScope.UsingAsync(async scope =>
                {
                    var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                    var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

                    // at this point we apply the action on the given tenant
                    await action(new FeatureService(shellFeatureManager, extensionManager), settings);
                });

                return;
            }

            // at this point we apply the action on the current tenant
            await action(new FeatureService(_shellFeaturesManager, _extensionManager), null);
        }

        private string GetNextUrl(string tenant)
        {
            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            if (!String.IsNullOrWhiteSpace(tenant))
            {
                return Url.Action(nameof(Features), new { tenant });
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
