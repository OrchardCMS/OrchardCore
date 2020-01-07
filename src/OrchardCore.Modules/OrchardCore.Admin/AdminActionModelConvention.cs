using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Admin
{
    /// <summary>
    /// Routing convention to apply admin constraint to mapped admin routes only.
    /// </summary>
    public class AdminActionModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerName == "Admin" || action.Controller.Attributes.OfType<AdminAttribute>().Any())
            {
                var factory = new AdminActionConstraintFactory();
                foreach (var selector in action.Selectors)
                {
                    selector.ActionConstraints.Add(factory);
                }
            }
        }
    }
}
