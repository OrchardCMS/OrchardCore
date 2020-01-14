using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Admin
{
    public class AdminFolderModelConvention : IPageRouteModelConvention
    {
        private readonly string _adminUrlPrefixTemplate;
        public AdminFolderModelConvention(string adminUrlPrefix){
            _adminUrlPrefixTemplate = adminUrlPrefix;
        }
        public void Apply(PageRouteModel model)
        {
            if(model.ViewEnginePath.IndexOf("/Admin/", StringComparison.OrdinalIgnoreCase) > -1 )
            {                
                var factory = new AdminActionConstraintFactory();
                for (var i = 0; i <  model.Selectors.Count; i++ )
                {
                    var selector = model.Selectors[i];                    
                    var template = selector.AttributeRouteModel.Template.Replace("/Admin/","/", StringComparison.OrdinalIgnoreCase);                    
                    selector.AttributeRouteModel.Template = AttributeRouteModel.CombineTemplates(_adminUrlPrefixTemplate,template);                    
                    selector.ActionConstraints.Add(factory);
                }
            }            
        }
    }
}