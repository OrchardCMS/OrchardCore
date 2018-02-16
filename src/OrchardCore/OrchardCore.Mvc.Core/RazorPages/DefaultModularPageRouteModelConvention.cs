using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Mvc.RazorPages
{
    public class DefaultModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultModularPageRouteModelConvention(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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

                    var extensionManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IExtensionManager>();
                    var name = extensionManager.GetExtension(module).Manifest.Name;

                    if (!String.IsNullOrWhiteSpace(name))
                    {
                        module = name;
                    }

                    template = module + pageName.Substring(pathIndex + "Pages".Length);

                    model.Selectors.Add(new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = template,
                            Name = template.Replace('/', '.')
                        }
                    });

                    break;
                }

                pathIndex += segment.Length + 1;
            }
        }
    }
}
