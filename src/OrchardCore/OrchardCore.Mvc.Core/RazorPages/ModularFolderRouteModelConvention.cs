using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularFolderRouteModelConvention : IPageRouteModelConvention
    {
        private readonly string _folderPath;
        private readonly string _route;

        public ModularFolderRouteModelConvention(string folderPath, string route)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                _folderPath = folderPath.Trim('/');
                _route = route.Trim('/');
            }
        }

        public void Apply(PageRouteModel model)
        {
            if (_folderPath == null)
            {
                return;
            }

            var index = model.ViewEnginePath.LastIndexOf('/');

            if (index == -1)
            {
                return;
            }

            var fileName = model.ViewEnginePath.Substring(index + 1);
            var pageName = _folderPath + '/' + fileName;

            if (model.ViewEnginePath.EndsWith('/' + pageName))
            {
                foreach (var selector in model.Selectors)
                {
                    selector.AttributeRouteModel.SuppressLinkGeneration = true;
                }

                var template = _route + '/' + fileName;

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

    public static partial class PageConventionCollectionExtensions
    {
        public static PageConventionCollection AddModularFolderRoute(this PageConventionCollection conventions, string folderPath, string route)
        {
            conventions.Add(new ModularFolderRouteModelConvention(folderPath, route));
            return conventions;
        }
    }
}
