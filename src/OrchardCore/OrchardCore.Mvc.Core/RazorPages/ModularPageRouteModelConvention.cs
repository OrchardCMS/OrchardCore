using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template.Replace("/Pages/", "/");
            }
        }
    }
}
