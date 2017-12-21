using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class DefaultModularPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                var template = selector.AttributeRouteModel.Template;

                if (template.Contains("/Pages/") && !template.StartsWith("/Pages/"))
                {
                    var pageIndex = template.LastIndexOf("/Pages/", StringComparison.Ordinal);
                    var moduleFolder = template.Substring(0, pageIndex);
                    var moduleId = moduleFolder.Substring(moduleFolder.LastIndexOf('/') + 1);

                    template = moduleId + template.Replace("/Pages/", "/").Substring(pageIndex);
                    selector.AttributeRouteModel.Name = template.Replace('/', '.');
                    selector.AttributeRouteModel.Template = template;
                }
            }
        }
    }
}
