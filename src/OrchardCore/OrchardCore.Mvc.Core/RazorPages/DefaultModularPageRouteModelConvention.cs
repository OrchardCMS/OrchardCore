using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class DefaultModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public DefaultModularPageRouteModelConvention(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Apply(PageRouteModel model)
        {
            var pageName = model.ViewEnginePath.Trim('/');
            var tokenizer = new StringTokenizer(pageName, new[] { '/' });
            int count = tokenizer.Count(), pathIndex = 0;

            for (var i = 0; i < count; i++)
            {
                var segment = tokenizer.ElementAt(i);

                if ("Pages" == segment)
                {
                    if (i < 2 || i == count - 1)
                    {
                        return;
                    }

                    foreach (var selector in model.Selectors)
                    {
                        selector.AttributeRouteModel.SuppressLinkGeneration = true;
                    }

                    var module = tokenizer.ElementAt(i - 1).Value;

                    var template = pageName.Substring(pathIndex - (module.Length + 1));

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = template,
                            Name = template.Replace('/', '.')
                        }
                    });

                    var attributeRouteModel = new AttributeRouteModel();

                    if (module != _hostingEnvironment.ApplicationName)
                    {
                        attributeRouteModel.Template = module + pageName.Substring(pathIndex + "Pages".Length);
                        attributeRouteModel.Name = attributeRouteModel.Template.Replace('/', '.');
                    }
                    else
                    {
                        attributeRouteModel.Template = pageName.Substring(pathIndex + "Pages".Length + 1);

                        // When a Page named "Index" is defined in the application's module
                        // we force the homepage template.
                        if (String.Equals(attributeRouteModel.Template, "Index", StringComparison.OrdinalIgnoreCase))
                        {
                            attributeRouteModel.Template = "";
                            attributeRouteModel.Name = "Index";
                        }
                        else
                        {                            
                            attributeRouteModel.Name = attributeRouteModel.Template.Replace('/', '.');
                        }

                    }

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = attributeRouteModel
                    });

                    break;
                }

                pathIndex += segment.Length + 1;
            }
        }
    }
}
