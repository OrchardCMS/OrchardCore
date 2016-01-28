using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc;

namespace Orchard.Hosting.Routing
{
    /// <summary>
    /// Adds an area route constraint using the name of the module.
    /// </summary>
    public class ModuleAreaRouteConstraintConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach(var controller in application.Controllers)
            {
                var module = controller.ControllerType.Assembly.FullName;
                var splitIndex = module.IndexOf(',');
                if (splitIndex > 0)
                {
                    module = module.Substring(0, splitIndex);
                }

                controller.RouteConstraints.Add(new AreaAttribute(module));
            }
        }
    }
}
