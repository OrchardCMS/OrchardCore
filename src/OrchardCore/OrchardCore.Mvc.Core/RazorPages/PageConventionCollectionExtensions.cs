using System;
using System.IO;
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
        /// and whose razor view file name doesn't start with 'Admin'.
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
        /// or whose razor view file name starts with 'Admin'.
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
                if (isAdmin != (model.ViewEnginePath.Contains("/Admin/") || Path.GetFileName(model.ViewEnginePath).StartsWith("Admin", StringComparison.Ordinal)))
                {
                    return;
                }

                var areaFolder = areaName + folderPath;

                foreach (var selector in model.Selectors.ToArray())
                {
                    if (selector.AttributeRouteModel.Template.StartsWith(areaFolder, StringComparison.Ordinal))
                    {
                        selector.AttributeRouteModel.SuppressLinkGeneration = true;

                        var template = (folderRoute.Trim('/') + '/' + selector.AttributeRouteModel
                            .Template.Substring(areaFolder.Length).TrimStart('/')).TrimEnd('/');

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
