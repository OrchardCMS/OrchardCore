using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                if (selector.AttributeRouteModel.Template.Contains("/Pages/") &&
                    !selector.AttributeRouteModel.Template.StartsWith("/Pages/"))
                {
                    selector.AttributeRouteModel.Template = selector.AttributeRouteModel.
                        Template.Replace("/Pages/", "/");
                }
            }
        }
    }
}
