using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules.Routing
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// </summary>
    public class ModuleAreaRouteConstraintApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public ModuleAreaRouteConstraintApplicationModelProvider(ITypeFeatureProvider typeFeatureProvider)
        {
            _typeFeatureProvider = typeFeatureProvider;
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
                var feature = _typeFeatureProvider.GetFeatureForDependency(controller.ControllerType.AsType());
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
