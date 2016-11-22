using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using System.IO;
using System.Linq;

namespace Orchard.Hosting.Mvc.Razor
{
    public class ModuleViewLocationExpander : IViewLocationExpander
    {
        private readonly IDictionary<string,string> _moduleFolderPerAreaName;
        public ModuleViewLocationExpander(IEnumerable<string> extraModulesFolders)
        {
            _moduleFolderPerAreaName = extraModulesFolders.Union(new List<string>() { "Modules" })
                               .SelectMany(folder => Directory.GetDirectories(folder)
                                    .Select(subfolder => 
                                            new
                                            {
                                                ModuleFolder = folder,
                                                AreaName = Path.GetFileName(subfolder)
                                            } ))
                                    .ToDictionary(area=>area.AreaName,area=>area.ModuleFolder);            
        }
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var result = new List<string>();
            
            result.Add("/" + _moduleFolderPerAreaName[context.AreaName] + "/{2}/Views/{1}/{0}.cshtml");
            result.Add("/" + _moduleFolderPerAreaName[context.AreaName] + "/{2}/Views/Shared/{0}.cshtml");

            result.AddRange(viewLocations);

            return result;
        }
    }
}