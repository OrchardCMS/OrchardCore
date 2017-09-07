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
                    var pageIndex = template.LastIndexOf("/Pages/");
                    var moduleFolder = template.Substring(0, pageIndex);
                    var moduleId = moduleFolder.Substring(moduleFolder.LastIndexOf("/") + 1);

                    selector.AttributeRouteModel.Template = moduleId + template
                        .Replace("/Pages/", "/").Substring(pageIndex);
                }
            }
        }
    }
}
