using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRouteModelConvention : IPageRouteModelConvention
    {
        private readonly string _pageName;
        private readonly string _route;

        public ModularPageRouteModelConvention(string pageName, string route)
        {
            _pageName = pageName?.Trim('/');
            _route = route?.Trim('/');
        }

        public void Apply(PageRouteModel model)
        {
            if (_pageName == null || _route == null)
            {
                return;
            }

            var pageName = model.ViewEnginePath.Trim('/');

            if (pageName.EndsWith('/' + _pageName))
            {
                var selectors = model.Selectors.ToArray();

                foreach (var selector in selectors)
                {
                    selector.AttributeRouteModel.SuppressLinkGeneration = true;
                    var pageTemplate = selector.AttributeRouteModel.Template;

                    if (pageTemplate.Equals(_pageName) || pageTemplate.StartsWith(_pageName + '/'))
                    {
                        var template = pageTemplate.Replace(_pageName, _route).TrimStart('/');

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
        }
    }

    public static partial class PageConventionCollectionExtensions
    {
        public static PageConventionCollection AddModularPageRoute(this PageConventionCollection conventions, string pageName, string route)
        {
            conventions.Add(new ModularPageRouteModelConvention(pageName, route));
            return conventions;
        }
    }
}
