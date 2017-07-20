using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement.Fluid;
using Orchard.DisplayManagement.Theming;

namespace Orchard.Templates.Services
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : IFileProvider
    {
        private TemplatesManager _templatesManager;
        private IThemeManager _themanager;

        public TemplateFileProvider(TemplatesManager templatesManager, IThemeManager themanager)
        {
            _templatesManager = templatesManager;
            _themanager = themanager;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var entries = new List<IFileInfo>();

            var themePath = _themanager.GetThemeAsync().GetAwaiter().GetResult().SubPath.Replace("\\", "/");

            if (subpath.EndsWith(string.Format("{0}/{1}", themePath, "Views")))
            {
                var templatesDocument = _templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();

                foreach (var template in templatesDocument.Templates)
                {
                    entries.Add(new DisplayManagement.Fluid.Internal.ContentFileInfo(
                        string.Format("{0}{1}", template.Key, FluidViewTemplate.ViewExtension),
                        template.Value.Content));
                }
            }

            return new EnumerableDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (Path.GetExtension(subpath) != FluidViewTemplate.ViewExtension)
            {
                return null;
            }

            var name = Path.GetFileNameWithoutExtension(subpath);

            var templatesDocument = _templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();

            if (templatesDocument.Templates.TryGetValue(name, out var template))
            {
                return new DisplayManagement.Fluid.Internal.ContentFileInfo(
                    string.Format("{0}{1}", name, FluidViewTemplate.ViewExtension),
                    template.Content);
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (Path.GetExtension(filter) != FluidViewTemplate.ViewExtension)
            {
                return null;
            }

            var name = Path.GetFileNameWithoutExtension(filter);

            var templatesDocument = _templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();

            if (templatesDocument.Templates.TryGetValue(name, out var template))
            {
                return _templatesManager.ChangeToken;
            }

            return null;
        }
    }
}
