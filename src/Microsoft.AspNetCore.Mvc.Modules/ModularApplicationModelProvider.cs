using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Orchard.Environment.Shell.Builders.Models;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// </summary>
    public class ModularApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ShellBlueprint _shellBlueprint;

        public ModularApplicationModelProvider(ShellBlueprint shellBlueprint)
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
            // This code is called only once per tenant during the construction of routes
            foreach (var controller in context.Result.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();
                var blueprint = _shellBlueprint.Dependencies.FirstOrDefault(dep => dep.Type == controllerType);
                if (blueprint != null)
                {
                    controller.RouteValues.Add("area", blueprint.Feature.Extension.Id);
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }
    }
}
