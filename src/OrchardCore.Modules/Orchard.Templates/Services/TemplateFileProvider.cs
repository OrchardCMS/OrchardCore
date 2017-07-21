using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement.FileProviders;
using Orchard.Settings;

namespace Orchard.Templates.Services
{
    public interface ITemplateFileProvider : IFileProvider { }

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : ITemplateFileProvider
    {
        private readonly string _themeViewsPath;
        private readonly TemplatesManager _templatesManager;

        public TemplateFileProvider(ISiteService siteService, TemplatesManager templatesManager)
        {
            _themeViewsPath = string.Format("{0}/{1}", (string)siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult().Properties["CurrentThemeName"], "Views");

            _templatesManager = templatesManager;
        }

        private Dictionary<string, Models.Template> Templates =>
            _templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult().Templates;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var entries = new List<IFileInfo>();

            if (subpath.EndsWith(_themeViewsPath))
            {
                foreach (var template in Templates)
                {
                    entries.Add(new ContentFileInfo(template.Key, template.Value.Content));
                }
            }

            return new EnumerableDirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath.Contains(_themeViewsPath))
            {
                var name = GetTemplateName(subpath);

                if (Templates.TryGetValue(GetTemplateName(subpath), out var template))
                {
                    return new ContentFileInfo(name, template.Content);
                }
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (filter.Contains(_themeViewsPath))
            {
                if (Templates.TryGetValue(GetTemplateName(filter), out var template))
                {
                    return _templatesManager.ChangeToken;
                }
            }

            return null;
        }

        private string GetTemplateName(string path)
        {
            return path.Substring(path.IndexOf(_themeViewsPath) + _themeViewsPath.Length + 1);
        }
    }
}
