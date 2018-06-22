using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Features.Models;
using OrchardCore.Features.Services;
using OrchardCore.Features.ViewModels;
using OrchardCore.Mvc.ActionConstraints;

namespace OrchardCore.Features.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IModuleService _moduleService;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;

        public AdminController(
            IModuleService moduleService,
            IExtensionManager extensionManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellDescriptorManager shellDescriptorManager,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            ShellSettings shellSettings,
            INotifier notifier)
        {
            _moduleService = moduleService;
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _shellFeaturesManager = shellFeaturesManager;
            _authorizationService = authorizationService;
            _shellSettings = shellSettings;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<ActionResult> Features()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures)) // , T["Not allowed to manage features."]
            {
                return Unauthorized();
            }

            var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

            var moduleFeatures = new List<ModuleFeature>();
            foreach (var moduleFeatureInfo in _extensionManager
                .GetFeatures()
                .Where(f => !f.Extension.IsTheme() && FeatureIsAllowed(f)))
            {
                var dependentFeatures = _extensionManager.GetDependentFeatures(moduleFeatureInfo.Id);
                var featureDependencies = _extensionManager.GetFeatureDependencies(moduleFeatureInfo.Id);

                var moduleFeature = new ModuleFeature
                {
                    Descriptor = moduleFeatureInfo,
                    IsEnabled = enabledFeatures.Contains(moduleFeatureInfo),
                    //IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(f.Extension),
                    //NeedsUpdate = featuresThatNeedUpdate.Contains(f.Id),
                    DependentFeatures = dependentFeatures.Where(x => x.Id != moduleFeatureInfo.Id).ToList(),
                    FeatureDependencies = featureDependencies.Where(d => d.Id != moduleFeatureInfo.Id).ToList()
                };

                moduleFeatures.Add(moduleFeature);
            }

            return View(new FeaturesViewModel
            {
                Features = moduleFeatures,
                IsAllowed = FeatureIsAllowed
            });
        }

        [HttpPost, ActionName("Features")]
        [FormValueRequired("submit.BulkExecute")]
        public async Task<ActionResult> FeaturesPOST(FeaturesBulkAction bulkAction, IList<string> featureIds, bool? force)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures))
            {
                return Unauthorized();
            }

            if (featureIds == null || !featureIds.Any())
            {
                ModelState.AddModelError("featureIds", T["Please select one or more features."].ToString());
            }

            if (ModelState.IsValid)
            {
                var availableFeatures = _extensionManager.GetFeatures();
                var features = availableFeatures.Where(feature => FeatureIsAllowed(feature)).ToList();
                var selectedFeatures = features.Where(x => featureIds.Contains(x.Id)).ToList();
                var allEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync(); //features.Where(x => x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesEnabled = allEnabledFeatures.Where(x => featureIds.Contains(x.Id)).ToList();
                var allDisabledFeatures = await _shellFeaturesManager.GetDisabledFeaturesAsync(); // DisabledFeaturesAsync //features.Where(x => !x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesDisabled = allDisabledFeatures.Where(x => featureIds.Contains(x.Id)).ToList();

                switch (bulkAction)
                {
                    case FeaturesBulkAction.None:
                        break;
                    case FeaturesBulkAction.Enable:
                        var enabledFeatures = await _shellFeaturesManager.EnableFeaturesAsync(idFeaturesDisabled, force == true);
                        foreach (var feature in enabledFeatures.ToList())
                        {
                            var featureName = availableFeatures.First(fi => fi.Id == feature.Id).Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Disable:
                        var disabledFeatures = await _shellFeaturesManager.DisableFeaturesAsync(idFeaturesEnabled, force == true);
                        foreach (var feature in disabledFeatures.ToList())
                        {
                            var featureName = availableFeatures.First(fi => fi.Id == feature.Id).Name;
                            _notifier.Success(T["{0} was disabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Toggle:
                        var enabledFeaturesToggle = await _shellFeaturesManager.EnableFeaturesAsync(idFeaturesDisabled, force == true);
                        foreach (var feature in enabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures.First(fi => fi.Id == feature.Id).Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }

                        var disabledFeaturesToggle = await _shellFeaturesManager.DisableFeaturesAsync(idFeaturesEnabled, force == true);
                        foreach (var feature in disabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures.First(fi => fi.Id == feature.Id).Name;
                            _notifier.Success(T["{0} was disabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Update:
                        //var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();
                        //var selectedFeaturesThatNeedUpdate = selectedFeatures.Where(x => featuresThatNeedUpdate.Contains(x.Id));

                        //foreach (var feature in selectedFeaturesThatNeedUpdate)
                        //{
                        //    var id = feature.Descriptor.Id;
                        //    try
                        //    {
                        //        _dataMigrationManager.Update(id);
                        //        _notifier.Success(T["The feature {0} was updated successfully", id]);
                        //    }
                        //    catch (Exception exception)
                        //    {
                        //        _notifier.Error(T["An error occurred while updating the feature {0}: {1}", id, exception.Message]);
                        //    }
                        //}
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("Features");
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string id)
        {
            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => FeatureIsAllowed(f) && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _shellFeaturesManager.DisableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(T["{0} was disabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            var feature = _extensionManager.GetFeatures().FirstOrDefault(f => FeatureIsAllowed(f) && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _shellFeaturesManager.EnableFeaturesAsync(new[] { feature }, force: true);

            _notifier.Success(T["{0} was enabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        /// <summary>
        /// Checks whether the feature is allowed for the current tenant
        /// </summary>
        private bool FeatureIsAllowed(IFeatureInfo feature)
        {
            // TODO: Implement white-list of modules allowed in the shell settings

            // Checks if the feature is only allowed on the Default tenant
            return _shellSettings.Name == ShellHelper.DefaultShellName || !feature.DefaultTenantOnly;
        }
    }
}