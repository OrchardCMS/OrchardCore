using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class DefaultModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly IApplicationContext _applicationContext;

        public DefaultModularPageRouteModelConvention(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Apply(PageRouteModel model)
        {
            var selectors = model.Selectors.ToArray();

            foreach (var selector in selectors)
            {
                var pageTemplate = selector.AttributeRouteModel.Template;
                var tokenizer = new StringTokenizer(pageTemplate, new[] { '/' });
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

                        selector.AttributeRouteModel.SuppressLinkGeneration = true;

                        var module = tokenizer.ElementAt(i - 1).Value;

                        // "{ModuleId}/Pages/Foo" - "{ApplicationName}/Pages/Foo"
                        var template = pageTemplate.Substring(pathIndex - (module.Length + 1));

                        model.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel
                            {
                                Template = template,
                                Name = template.Replace('/', '.')
                            }
                        });

                        var attributeRouteModel = new AttributeRouteModel();

                        if (module != _applicationContext.Application.Name)
                        {
                            // "{ModuleId}/Foo".
                            attributeRouteModel.Template = module + pageTemplate.Substring(pathIndex + "Pages".Length);
                            attributeRouteModel.Name = attributeRouteModel.Template.Replace('/', '.');
                        }
                        else
                        {
                            // "Foo"
                            attributeRouteModel.Template = pageTemplate.Substring(pathIndex + "Pages".Length + 1);

                            // Check if a Page named "Index" is defined in the application's module,
                            if (String.Equals(attributeRouteModel.Template, "Index", StringComparison.OrdinalIgnoreCase))
                            {
                                // Force the homepage template.
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
}
