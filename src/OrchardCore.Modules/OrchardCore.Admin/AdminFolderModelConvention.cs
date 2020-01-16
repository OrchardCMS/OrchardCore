using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Admin
{
    public class AdminFolderModelConvention : IPageRouteModelConvention
    {
        private readonly string _adminUrlPrefixTemplate;

        public AdminFolderModelConvention(string adminUrlPrefix)
        {
            _adminUrlPrefixTemplate = adminUrlPrefix;
        }
        public void Apply(PageRouteModel model)
        {
            if(model.ViewEnginePath.Contains("/Admin/"))
            {
                var factory = new AdminActionConstraintFactory();

                foreach(var selector in model.Selectors)
                {
                    var template = selector.AttributeRouteModel.Template.Replace("/Admin/","/");
                    selector.AttributeRouteModel.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate,template);
                    selector.ActionConstraints.Add(factory);
                }
            }
        }
    }
}
