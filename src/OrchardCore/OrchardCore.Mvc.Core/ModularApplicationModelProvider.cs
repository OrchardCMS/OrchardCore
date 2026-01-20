using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// And filters all controller actions of disabled features.
    /// </summary>
    public class ModularApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ShellSettings _shellSettings;

        public ModularApplicationModelProvider(
            ITypeFeatureProvider typeFeatureProvider,
            IHostEnvironment hostingEnvironment,
            ShellSettings shellSettings)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _hostingEnvironment = hostingEnvironment;
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

                if (blueprint != null)
                {
                    if (blueprint.Extension.Id == _hostingEnvironment.ApplicationName && !_shellSettings.IsRunning())
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
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }
    }
}
