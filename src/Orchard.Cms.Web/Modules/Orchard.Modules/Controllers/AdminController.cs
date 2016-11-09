using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.Admin;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Modules.Models;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Mvc;

namespace Orchard.Modules.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IModuleService _moduleService;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            IModuleService moduleService,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellDescriptorManager shellDescriptorManager,
            IAuthorizationService authorizationService,
            INotifier notifier)
        {
            _moduleService = moduleService;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _shellDescriptorManager = shellDescriptorManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<ActionResult> Index()
        {
            IEnumerable<ModuleEntry> modules = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => DefaultExtensionTypes.IsModule(extensionDescriptor.ExtensionType))
                .OrderBy(extensionDescriptor => extensionDescriptor.Name)
                .Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor });
            
            var features = await _featureManager.GetEnabledFeaturesAsync();
            var installModules = features.FirstOrDefault(f => f.Id == "PackagingServices") != null;

            modules = modules.ToList();
            foreach (ModuleEntry moduleEntry in modules)
            {
                moduleEntry.IsRecentlyInstalled = false; //_moduleService.IsRecentlyInstalled(moduleEntry.Descriptor);
                moduleEntry.CanUninstall = installModules;

                //if (_extensionDisplayEventHandler != null)
                //{
                //    foreach (string notification in _extensionDisplayEventHandler.Displaying(moduleEntry.Descriptor, ControllerContext.RequestContext))
                //    {
                //        moduleEntry.Notifications.Add(notification);
                //    }
                //}
            }

            var model = new ModulesIndexViewModel { 
                Modules = modules,
                InstallModules = installModules
            };

            return View(model);
        }

        public async Task<ActionResult> Features()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageFeatures)) // , T["Not allowed to manage features."]
            {
                return Unauthorized();
            }

            //var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();
            var shellDescriptor = await _shellDescriptorManager.GetShellDescriptorAsync();
            var availableFeatures = await _featureManager.GetAvailableFeaturesAsync();

            IEnumerable<ModuleFeature> features = availableFeatures
                .Where(f => !DefaultExtensionTypes.IsTheme(f.Extension.ExtensionType))
                .Select(f => new ModuleFeature
                {
                    Descriptor = f,
                    IsEnabled = shellDescriptor.Features.Any(sf => sf.Name == f.Id),
                    //IsRecentlyInstalled = _moduleService.IsRecentlyInstalled(f.Extension),
                    //NeedsUpdate = featuresThatNeedUpdate.Contains(f.Id),
                    DependentFeatures = _moduleService.GetDependentFeatures(f.Id).Where(x => x.Id != f.Id).ToList()
                })
                .ToList();

            return View(new FeaturesViewModel
            {
                Features = features,
                IsAllowed = ExtensionIsAllowed
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
                var availableFeatures = await _featureManager.GetAvailableFeaturesAsync();
                var features = availableFeatures.Where(feature => ExtensionIsAllowed(feature.Extension)).ToList();
                var selectedFeatures = features.Where(x => featureIds.Contains(x.Id)).ToList();
                var allEnabledFeatures = await _featureManager.GetEnabledFeaturesAsync(); //features.Where(x => x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesEnabled = allEnabledFeatures.Where(x => featureIds.Contains(x.Id)).Select(x => x.Id).ToList();
                var allDisabledFeatures = await _featureManager.GetDisabledFeaturesAsync(); //features.Where(x => !x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesDisabled = allDisabledFeatures.Where(x => featureIds.Contains(x.Id)).Select(x => x.Id).ToList();

                switch (bulkAction)
                {
                    case FeaturesBulkAction.None:
                        break;
                    case FeaturesBulkAction.Enable:
                        //_moduleService.EnableFeatures(featuresToDisable, force == true);

                        var enabledFeatures = await _featureManager.EnableFeaturesAsync(idFeaturesDisabled, force == true);
                        foreach (string featureId in enabledFeatures.ToList())
                        {
                            var featureName = availableFeatures.Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Disable:
                        //_moduleService.DisableFeatures(enabledFeatures, force == true);

                        var disabledFeatures = await _featureManager.DisableFeaturesAsync(idFeaturesEnabled, force == true);
                        foreach (string featureId in disabledFeatures.ToList())
                        {
                            var featureName = availableFeatures.Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                            _notifier.Success(T["{0} was disabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Toggle:
                        //_moduleService.EnableFeatures(idFeaturesDisabled, force == true);
                        //_moduleService.DisableFeatures(idFeaturesEnabled, force == true);

                        var enabledFeaturesToggle = await _featureManager.EnableFeaturesAsync(idFeaturesDisabled, force == true);
                        foreach (string featureId in enabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures.Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }

                        var disabledFeaturesToggle = await _featureManager.DisableFeaturesAsync(idFeaturesEnabled, force == true);
                        foreach (string featureId in disabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures.Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
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
            var availableFeatures = await _featureManager.GetAvailableFeaturesAsync();
            var feature = availableFeatures.FirstOrDefault(f => ExtensionIsAllowed(f.Extension) && f.Id == id);
            
            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _featureManager.DisableFeaturesAsync(new[] { feature.Id }, force: true);

            _notifier.Success(T["{0} was disabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string id)
        {
            var availableFeatures = await _featureManager.GetAvailableFeaturesAsync();
            var feature = availableFeatures.FirstOrDefault(f => ExtensionIsAllowed(f.Extension) && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            await _featureManager.EnableFeaturesAsync(new[] { feature.Id }, force: true);

            _notifier.Success(T["{0} was enabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        /// <summary>
        /// Checks whether the module is allowed for the current tenant
        /// </summary>
        private bool ExtensionIsAllowed(ExtensionDescriptor extensionDescriptor)
        {
            return true; //_shellSettings.Modules.Length == 0 || _shellSettings.Modules.Contains(extensionDescriptor.Id);
        }
    }
}