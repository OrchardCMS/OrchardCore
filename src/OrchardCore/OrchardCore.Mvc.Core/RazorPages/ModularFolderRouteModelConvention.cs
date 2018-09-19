using System.Linq;
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
            var subpath = _folderPath + '/' + fileName;

            if (pageName.EndsWith('/' + subpath))
            {
                var selectors = model.Selectors.ToArray();

                foreach (var selector in selectors)
                {
                    selector.AttributeRouteModel.SuppressLinkGeneration = true;
                    var pageTemplate = selector.AttributeRouteModel.Template;

                    if (pageTemplate.StartsWith(subpath + '/'))
                    {
                        var template = pageTemplate.Replace(_folderPath, _route).TrimStart('/');

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
        public static PageConventionCollection AddModularFolderRoute(this PageConventionCollection conventions, string folderPath, string route)
        {
            conventions.Add(new ModularFolderRouteModelConvention(folderPath, route));
            return conventions;
        }
    }
}
