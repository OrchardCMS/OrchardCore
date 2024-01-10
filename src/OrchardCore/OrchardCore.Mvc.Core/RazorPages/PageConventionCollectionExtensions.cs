using System;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public static class PageConventionCollectionExtensions
    {
        /// <summary>
        /// Adds a folder route for all pages in the specified area and under the specified area folder.
        /// These pages can be routed via their names prefixed by folder route in addition to the default
        /// set of path based routes. Links generated for these pages will use the specified folder route.
        /// Note: Applied to all pages whose razor view file path doesn't contain any '/Admin/' segment
        /// and whose route model properties doesn't contains an 'Admin' key.
        /// </summary>
        public static PageConventionCollection AddAreaFolderRoute(this PageConventionCollection conventions,
            string areaName, string folderPath, string folderRoute)
        {
            return conventions.AddAreaFolderRouteInternal(areaName, folderPath, folderRoute, isAdmin: false);
        }

        /// <summary>
        /// Adds a folder route for all pages in the specified area and under the specified area folder.
        /// These pages can be routed via their names prefixed by folder route in addition to the default
        /// set of path based routes. Links generated for these pages will use the specified folder route.
        /// Note: Applied to all pages whose razor view file path contains an '/Admin/' segment
        /// or whose route model properties contains an 'Admin' key.
        /// </summary>
        public static PageConventionCollection AddAdminAreaFolderRoute(this PageConventionCollection conventions,
            string areaName, string folderPath, string folderRoute)
        {
            return conventions.AddAreaFolderRouteInternal(areaName, folderPath, folderRoute, isAdmin: true);
        }

        internal static PageConventionCollection AddAreaFolderRouteInternal(this PageConventionCollection conventions,
            string areaName, string folderPath, string folderRoute, bool isAdmin)
        {
            conventions.AddAreaFolderRouteModelConvention(areaName, folderPath, model =>
            {
                if (isAdmin != (model.ViewEnginePath.Contains("/Admin/") || model.Properties.ContainsKey("Admin")))
                {
                    return;
                }

                var areaFolder = areaName + folderPath;

                foreach (var selector in model.Selectors.ToArray())
                {
                    var route = selector.AttributeRouteModel;

                    if (route.Template.StartsWith(areaFolder, StringComparison.Ordinal) || (route.Template == areaName && folderPath == "/"))
                    {
                        route.SuppressLinkGeneration = true;

                        string template;

                        if (route.Template == areaName && folderPath == "/")
                        {
                            template = folderRoute;
                        }
                        else
                        {
                            var cleanSubTemplate = route.Template[areaFolder.Length..].TrimStart('/');
                            template = AttributeRouteModel.CombineTemplates(folderRoute, cleanSubTemplate);
                        }

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
