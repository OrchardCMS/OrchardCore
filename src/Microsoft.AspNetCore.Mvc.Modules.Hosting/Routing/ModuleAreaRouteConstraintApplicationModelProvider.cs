using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Orchard.Environment.Shell.Builders.Models;

namespace Orchard.Hosting.Routing
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// </summary>
    public class ModuleAreaRouteConstraintApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ShellBlueprint _shellBlueprint;

        public ModuleAreaRouteConstraintApplicationModelProvider(ShellBlueprint shellBlueprint)
        {
            _shellBlueprint = shellBlueprint;
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
            foreach (var controller in context.Result.Controllers)
            {
                var feature = _shellBlueprint.GetFeatureForDependency(controller.ControllerType.AsType());
                if (feature != null)
                {
                    controller.RouteValues.Add("area", feature.Extension.Id);
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }
    }
}
