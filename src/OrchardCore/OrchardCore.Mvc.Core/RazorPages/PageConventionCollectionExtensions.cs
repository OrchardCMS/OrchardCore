using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public static class PageConventionCollectionExtensions
    {
        /// <summary>
        /// Adds a folder route for all pages in the specified area and under the specified area folder.
        /// These pages can be routed via their names prefixed by folder route in addition to the default
        /// set of path based routes. Links generated for these pages will use the specified folder route.
        /// </summary>
        public static PageConventionCollection AddAreaFolderRoute(this PageConventionCollection conventions,
            string areaName, string folderPath, string folderRoute)
        {
            conventions.AddAreaFolderRouteModelConvention(areaName, folderPath, model =>
            {
                foreach (var selector in model.Selectors.ToArray())
                {
                    if (selector.AttributeRouteModel.Template.StartsWith(areaName))
                    {
                        selector.AttributeRouteModel.SuppressLinkGeneration = true;

                        var template = (folderRoute.Trim('/') + '/' + selector.AttributeRouteModel
                            .Template.Substring(areaName.Length).TrimStart('/')).TrimEnd('/');

                        model.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel
                            {
                                Template = template
                            }
                        });
                    }
                }
            });

            return conventions;
        }
    }
}
