using System;
using System.IO;
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

            if (route.Template.Contains("/Admin/"))
            {
                var template = route.Template.Replace("/Admin/", "/");
                route.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate, template);
            }
            else if (Path.GetFileName(model.ViewEnginePath).StartsWith("Admin", StringComparison.Ordinal))
            {
                route.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate, route.Template);
            }
        }
    }
}
