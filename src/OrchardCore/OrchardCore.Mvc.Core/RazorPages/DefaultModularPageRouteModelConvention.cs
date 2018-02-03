using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class DefaultModularPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            var pageName = model.ViewEnginePath.Trim('/');
            var pagesIndex = pageName.LastIndexOf("/Pages/", StringComparison.Ordinal);

            if (pagesIndex == -1)
            {
                return;
            }

            var moduleFolder = pageName.Substring(0, pagesIndex);
            var moduleIndex = moduleFolder.LastIndexOf('/');

            if (moduleIndex == -1)
            {
                return;
            }

            foreach (var selector in model.Selectors)
            {
                selector.AttributeRouteModel.SuppressLinkGeneration = true;
            }

            var template = pageName.Substring(moduleIndex + 1);

            model.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = template,
                    Name = template.Replace('/', '.')
                }
            });

            template = template.Replace("/Pages/", "/");

            model.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = template,
                    Name = template.Replace('/', '.')
                }
            });

        }
    }
}
