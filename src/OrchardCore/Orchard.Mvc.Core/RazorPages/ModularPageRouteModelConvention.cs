using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private const string PageFolder = "/Pages";
        private const string PageSegment = PageFolder + "/";

        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                selector.AttributeRouteModel.Template = selector.AttributeRouteModel
                    .Template.Replace(PageSegment, "/");
            }
        }
    }
}
