using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement.Fluid;
using Orchard.Settings;
namespace Orchard.Templates.Services
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : IFileProvider
    {
        private TemplatesManager _templatesManager;
        private readonly ISiteService _siteService;

        public TemplateFileProvider(TemplatesManager templatesManager, ISiteService siteService)
        {
            _templatesManager = templatesManager;
            _siteService = siteService;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var entries = new List<IFileInfo>();

            var themeName = (string)_siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult().Properties["CurrentThemeName"];

            if (subpath.EndsWith(string.Format("{0}/{1}", themeName, "Views")))
            {
                var templatesDocument = _templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();

                foreach (var template in templatesDocument.Templates)
                {
                    entries.Add(new DisplayManagement.Fluid.Internal.ContentFileInfo(
                        //string.Format("{0}{1}", template.Key, FluidViewTemplate.ViewExtension),
                        template.Key,
                        template.Value.Content));
                }
            }

            return new EnumerableDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            //if (Path.GetExtension(subpath) != FluidViewTemplate.ViewExtension)
            //{
            //    return null;
            //}

            var themeName = (string)_siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult().Properties["CurrentThemeName"];

            var themeViewsPath = string.Format("{0}/Views/", themeName);

            if (subpath.Contains(themeViewsPath))
            {
                var index = subpath.IndexOf(themeViewsPath);
                var name = subpath.Substring(index + themeViewsPath.Length);

                if (_templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult()
                    .Templates.TryGetValue(name, out var template))
                {
                    return new DisplayManagement.Fluid.Internal.ContentFileInfo(
                        string.Format("{0}{1}", name, FluidViewTemplate.ViewExtension),
                        template.Content);
                }
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            //if (Path.GetExtension(subpath) != FluidViewTemplate.ViewExtension)
            //{
            //    return null;
            //}

            var themeName = (string)_siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult().Properties["CurrentThemeName"];

            var themeViewsPath = string.Format("{0}/Views/", themeName);

            if (filter.Contains(themeViewsPath))
            {
                var index = filter.IndexOf(themeViewsPath);
                var name = filter.Substring(index + themeViewsPath.Length);

                if (_templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult()
                    .Templates.TryGetValue(name, out var template))
                {
                    return _templatesManager.ChangeToken;
                }
            }

            return null;
        }
    }
}
