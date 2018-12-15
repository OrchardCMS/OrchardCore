using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// And filters all controller actions of disabled features.
    /// </summary>
    public class ModularApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEnumerable<string> _enabledFeatureIds;
        private readonly ShellSettings _shellSettings;

        public ModularApplicationModelProvider(
            ITypeFeatureProvider typeFeatureProvider,
            IHostingEnvironment hostingEnvironment,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            ShellSettings shellSettings)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _hostingEnvironment = hostingEnvironment;

            _enabledFeatureIds = extensionManager.GetFeatures().Where(f => shellDescriptor
                .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Id).ToArray();

            _shellSettings = shellSettings;
        }

        public int Order
        {
            get
            {
                return 1000;
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // This code is called only once per tenant during the construction of routes.
            // Or if an 'IActionDescriptorChangeProvider' tells that an action descriptor
            // has changed. E.g 'PageActionDescriptorChangeProvider' after any page update.

            foreach (var controller in context.Result.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();
                var blueprint = _typeFeatureProvider.GetFeatureForDependency(controllerType);

                // Only serve controller actions of enabled features.
                if (blueprint != null && _enabledFeatureIds.Contains(blueprint.Id))
                {
                    if (blueprint.Extension.Id == _hostingEnvironment.ApplicationName &&
                        _shellSettings.State != TenantState.Running)
                    {
                        // Don't serve any action of the application'module which is enabled during a setup.
                        foreach (var action in controller.Actions)
                        {
                            action.Selectors.Clear();
                        }

                        controller.Selectors.Clear();
                    }
                    else
                    {
                        // Add an "area" route value equal to the module id.
                        controller.RouteValues.Add("area", blueprint.Extension.Id);
                    }
                }
                else
                {
                    // Don't serve any action of disabled features.
                    foreach (var action in controller.Actions)
                    {
                        action.Selectors.Clear();
                    }

                    controller.Selectors.Clear();
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }
    }
}
