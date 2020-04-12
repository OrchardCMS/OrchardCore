using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Admin
{
    // Admin Page Route Model Convention
    public class AdminPageRouteModelConvention : IPageRouteModelConvention
    {
        // Variable for storing a string with a prefix
        private readonly string _adminUrlPrefixTemplate;

        // The class constructor sets the url prefix value
        public AdminPageRouteModelConvention(string adminUrlPrefix)
        {
            _adminUrlPrefixTemplate = adminUrlPrefix;
        }

        // Trying to apply the specified prefix to the route Template from the model
        public void Apply(PageRouteModel model)
        {
            // From the model selector we get the first or default route model
            var route = model.Selectors.FirstOrDefault()?.AttributeRouteModel;

            // If the route was not received, then interrupt the execution of the method
            if (route == null)
            {
                return;
            }

            // If the route Template does not start with the name of the model area,
            // then we interrupt the method
            if (!route.Template.StartsWith(model.AreaName))
            {
                return;
            }

            // If the route Template contains "/Admin/" or the Properties contain the "Admin" key
            // Then the route Template will be combined with the given prefix value in the Template
            if (route.Template.Contains("/Admin/") || model.Properties.ContainsKey("Admin"))
            {
                route.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate, route.Template);
            }
        }
    }
}
