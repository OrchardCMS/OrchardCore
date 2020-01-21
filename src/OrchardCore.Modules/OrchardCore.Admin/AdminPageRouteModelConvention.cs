using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Admin
{
    public class AdminPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly string _adminUrlPrefixTemplate;

        public AdminPageRouteModelConvention(string adminUrlPrefix)
        {
            _adminUrlPrefixTemplate = adminUrlPrefix;
        }

        public void Apply(PageRouteModel model)
        {
            var route = model.Selectors.ElementAt(0).AttributeRouteModel;

            if (!route.Template.StartsWith(model.AreaName))
            {
                return;
            }

            if (route.Template.Contains("/Admin/") || model.Properties.ContainsKey("Admin"))
            {
                route.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate, route.Template);
            }
        }
    }
}
