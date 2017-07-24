using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.FileProviders;
using Orchard.Environment.Extensions;

namespace Orchard.Templates.Services
{
    public interface ITemplateFileProvider : IShellFileProvider { }

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : ITemplateFileProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, string> _viewsFolders;
        private readonly IFileProvider _fileProvider;

        public TemplateFileProvider(
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IExtensionManager extensionManager)
        {
            _httpContextAccessor = httpContextAccessor;

            // waiting for manifest tags to be able to exclude admin and hidden themes
            _viewsFolders = extensionManager.GetExtensions().Where(e => e.Manifest.IsTheme()).ToDictionary(
                    e => string.Format("{0}/{1}", e.SubPath.Replace('\\', '/').Trim('/'), "Views"),
                    e => string.Format("{0}/{1}", e.Id, "Views"));

            _fileProvider = hostingEnvironment.ContentRootFileProvider;
        }

        private TemplatesManager TemplatesManager => _httpContextAccessor
            .HttpContext.RequestServices.GetService<TemplatesManager>();

        private Dictionary<string, Models.Template> Templates => TemplatesManager
            .GetTemplatesDocumentAsync().GetAwaiter().GetResult().Templates;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (_viewsFolders.TryGetValue(subpath.Trim('/'), out var viewsFolder))
            {
                var entries = new List<IFileInfo>();

                entries.AddRange(Templates.Where(kv => kv.Key.StartsWith(viewsFolder + '/')).Select(kvp =>
                    new ContentFileInfo(kvp.Key.Substring(viewsFolder.Length + 1), kvp.Value.Content)));

                return new DirectoryContents(entries);
            }

            return new DirectoryContents(_fileProvider.GetDirectoryContents(subpath).Where(f => f.IsDirectory));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (TryGetTemplatePath(subpath, out var path))
            {
                if (Templates.TryGetValue(path, out var template))
                {
                    return new ContentFileInfo(Path.GetFileName(path), template.Content);
                }
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (TryGetTemplatePath(filter, out var templatePath))
            {
                if (Templates.TryGetValue(templatePath, out var template))
                {
                    return TemplatesManager.ChangeToken;
                }
            }

            return null;
        }

        private bool TryGetTemplatePath(string subpath, out string templatePath)
        {
            /*var folder = _viewsFolders.FirstOrDefault(p => subpath.TrimStart('/').StartsWith(p.Key + '/'));

            if (folder.Key != null)
            {
                templatePath = string.Format("{0}/{1}", folder.Value, subpath.TrimStart('/').Substring(folder.Key.Length + 1));
                return true;
            }*/

            var key = _viewsFolders.Keys.FirstOrDefault(k => subpath.TrimStart('/').StartsWith(k + '/'));

            if (key != null)
            {
                templatePath = string.Format("{0}/{1}", _viewsFolders[key],
                    subpath.TrimStart('/').Substring(key.Length + 1));

                return true;
            }

            templatePath = null;
            return false;
        }
    }
}
