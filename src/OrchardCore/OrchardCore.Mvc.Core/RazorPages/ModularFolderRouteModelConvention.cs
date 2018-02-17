using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularFolderRouteModelConvention : IPageRouteModelConvention
    {
        private readonly string _folderPath;
        private readonly string _route;

        public ModularFolderRouteModelConvention(string folderPath, string route)
        {
             _folderPath = folderPath?.Trim('/');
            _route = route?.Trim('/');
        }

        public void Apply(PageRouteModel model)
        {
            if (_folderPath == null || _route == null)
            {
                return;
            }

            var pageName = model.ViewEnginePath.Trim('/');
            var fileIndex = pageName.LastIndexOf('/');

            if (fileIndex == -1)
            {
                return;
            }

            var fileName = pageName.Substring(fileIndex + 1);

            if (pageName.EndsWith('/' + _folderPath + '/' + fileName))
            {
                foreach (var selector in model.Selectors)
                {
                    selector.AttributeRouteModel.SuppressLinkGeneration = true;
                }

                var template = _route.Length > 0 ? _route + '/' + fileName : fileName;

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
