using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private const string PageFolder = "/Pages";

        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                selector.AttributeRouteModel.Template = selector.AttributeRouteModel
                    .Template.Replace(PageFolder + "/", "/");
            }
        }
    }
}
