using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
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
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly FeatureService _featureService;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            ShellSettings shellSettings,
            INotifier notifier,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _shellSettings = shellSettings;
            _notifier = notifier;
            H = localizer;
            S = stringLocalizer;
            _featureService = new FeatureService(shellFeaturesManager, extensionManager);
        }

        public async Task<ActionResult> Features()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            return View(new FeaturesViewModel
            {
                Features = await _featureService.GetModuleFeaturesAsync()
            });
        }

        [HttpPost]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> Features(BulkActionViewModel model, bool? force)
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
                var availableFeatures = await _featureService.GetAvailableFeatures(model.FeatureIds);

                await _featureService.EnableOrDisableFeaturesAsync(availableFeatures, model.BulkAction, force, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            }

            return RedirectToAction(nameof(Features));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var feature = await _featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Disable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));

            return Redirect(nextUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Forbid();
            }

            var feature = await _featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Enable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));

            return Redirect(nextUrl);
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
