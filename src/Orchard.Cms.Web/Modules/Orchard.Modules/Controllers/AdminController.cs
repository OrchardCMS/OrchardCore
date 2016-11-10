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
using Orchard.Environment.Shell.Descriptor;
using Orchard.Modules.Models;
using Orchard.Modules.Services;
using Orchard.Modules.ViewModels;
using Orchard.Mvc;
using Orchard.Environment.Shell;

namespace Orchard.Modules.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IModuleService _moduleService;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public AdminController(
            IModuleService moduleService,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            IHtmlLocalizer<AdminController> localizer,
            IShellDescriptorManager shellDescriptorManager,
            IShellFeaturesManager shellFeaturesManager,
            IAuthorizationService authorizationService,
            INotifier notifier)
        {
            _moduleService = moduleService;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _shellDescriptorManager = shellDescriptorManager;
            _shellFeaturesManager = shellFeaturesManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public ActionResult Index()
        {
            IEnumerable<ModuleEntry> modules = _extensionManager.GetExtensions()
                .Where(extensionDescriptor => extensionDescriptor.Manifest.IsModule())
                .OrderBy(extensionDescriptor => extensionDescriptor.Manifest.Name)
                .Select(extensionDescriptor => new ModuleEntry { Descriptor = extensionDescriptor });
            
            var features = _shellFeaturesManager.EnabledFeatures();
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
            var availableFeatures = _extensionManager.GetExtensions().Features;

            IEnumerable<ModuleFeature> features = availableFeatures
                .Where(f => !f.Extension.Manifest.IsTheme())
                .Select(f => new ModuleFeature
                {
                    Descriptor = f,
                    IsEnabled = shellDescriptor.Features.Any(sf => sf.Id == f.Id),
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
                var availableFeatures = _extensionManager.GetExtensions().Features;
                var features = availableFeatures.Where(feature => ExtensionIsAllowed(feature.Extension)).ToList();
                var selectedFeatures = features.Where(x => featureIds.Contains(x.Id)).ToList();
                var allEnabledFeatures = _shellFeaturesManager.EnabledFeatures(); //features.Where(x => x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesEnabled = allEnabledFeatures.Where(x => featureIds.Contains(x.Id)).ToList();
                var allDisabledFeatures = _shellFeaturesManager.DisabledFeatures(); // DisabledFeaturesAsync //features.Where(x => !x.IsEnabled && featureIds.Contains(x.Id)).Select(x => x.Descriptor.Id).ToList();
                var idFeaturesDisabled = allDisabledFeatures.Where(x => featureIds.Contains(x.Id)).ToList();

                switch (bulkAction)
                {
                    case FeaturesBulkAction.None:
                        break;
                    case FeaturesBulkAction.Enable:
                        //_moduleService.EnableFeatures(featuresToDisable, force == true);

                        var enabledFeatures = _shellFeaturesManager.EnableFeatures(idFeaturesDisabled, force == true);
                        foreach (var feature in enabledFeatures.ToList())
                        {
                            var featureName = availableFeatures[feature.Id].Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Disable:
                        //_moduleService.DisableFeatures(enabledFeatures, force == true);

                        var disabledFeatures = _shellFeaturesManager.DisableFeatures(idFeaturesEnabled, force == true);
                        foreach (var feature in disabledFeatures.ToList())
                        {
                            var featureName = availableFeatures[feature.Id].Name;
                            _notifier.Success(T["{0} was disabled", featureName]);
                        }
                        break;
                    case FeaturesBulkAction.Toggle:
                        //_moduleService.EnableFeatures(idFeaturesDisabled, force == true);
                        //_moduleService.DisableFeatures(idFeaturesEnabled, force == true);

                        var enabledFeaturesToggle = _shellFeaturesManager.EnableFeatures(idFeaturesDisabled, force == true);
                        foreach (var feature in enabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures[feature.Id].Name;
                            _notifier.Success(T["{0} was enabled", featureName]);
                        }

                        var disabledFeaturesToggle = _shellFeaturesManager.DisableFeatures(idFeaturesEnabled, force == true);
                        foreach (var feature in disabledFeaturesToggle.ToList())
                        {
                            var featureName = availableFeatures[feature.Id].Name;
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
        public IActionResult Disable(string id)
        {
            var availableFeatures = _extensionManager.GetExtensions().Features;
            var feature = availableFeatures.FirstOrDefault(f => ExtensionIsAllowed(f.Extension) && f.Id == id);
            
            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            _shellFeaturesManager.DisableFeatures(new[] { feature }, force: true);

            _notifier.Success(T["{0} was disabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        [HttpPost]
        public IActionResult Enable(string id)
        {
            var availableFeatures = _extensionManager.GetExtensions().Features;
            var feature = availableFeatures.FirstOrDefault(f => ExtensionIsAllowed(f.Extension) && f.Id == id);

            if (feature == null)
            {
                return NotFound();
            }

            // Generating routes can fail while the tenant is recycled as routes can use services.
            // It could be fixed by waiting for the next request or the end of the current one
            // to actually release the tenant. Right now we render the url before recycling the tenant.

            var nextUrl = Url.Action(nameof(Features));

            _shellFeaturesManager.EnableFeatures(new[] { feature }, force: true);

            _notifier.Success(T["{0} was enabled", feature.Name ?? feature.Id]);

            return Redirect(nextUrl);
        }

        /// <summary>
        /// Checks whether the module is allowed for the current tenant
        /// </summary>
        private bool ExtensionIsAllowed(IExtensionInfo extensionDescriptor)
        {
            return true; //_shellSettings.Modules.Length == 0 || _shellSettings.Modules.Contains(extensionDescriptor.Id);
        }
    }
}