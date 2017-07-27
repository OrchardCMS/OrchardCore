using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.FileProviders;
using Orchard.DisplayManagement.Fluid;
using Orchard.Environment.Extensions;

namespace Orchard.Templates.Services
{
    public interface ITemplateFileProvider : IShellFileProvider { }

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : ITemplateFileProvider
    {
        private static readonly string _fluidPageContent =
            "@using Orchard.DisplayManagement.Fluid;" + System.Environment.NewLine +
            "@inherits FluidPage" + System.Environment.NewLine +
            "@{ await RenderAsync(this); }";

        private static Dictionary<string, string> _themesViewsPaths;
        private static object _synLock = new object();

        private IChangeToken _templatesChangeToken;
        private IDictionary<string, Models.Template> _templates;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TemplateFileProvider(
            IHttpContextAccessor httpContextAccessor,
            IExtensionManager extensionManager)
        {
            _httpContextAccessor = httpContextAccessor;

            if (_themesViewsPaths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_themesViewsPaths == null)
                {
                    _themesViewsPaths = extensionManager.GetExtensions().Where(e => e.Manifest.IsTheme()).ToDictionary(
                        e => string.Format("{0}/{1}", e.SubPath.Replace('\\', '/').Trim('/'), "Views"),
                        e => string.Format("{0}/{1}", e.Id, "Views"));
                }
            }
        }

        private IDictionary<string, Models.Template> Templates => GetTemplates();

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            subpath = subpath.Replace("\\", "/").Trim('/');

            var entries = new List<IFileInfo>();

            if (_themesViewsPaths.TryGetValue(subpath, out var viewsFolder))
            {
                var templates = Templates.Where(kv => kv.Key.StartsWith(string.Format("{0}/", viewsFolder)));

                entries.AddRange(templates.Select(kvp => new ContentFileInfo(
                    kvp.Key.Substring(viewsFolder.Length + 1), kvp.Value.Content)));

                entries.AddRange(templates.Select(kvp => new ContentFileInfo(
                    Path.ChangeExtension(kvp.Key.Substring(viewsFolder.Length + 1), RazorViewEngine.ViewExtension),
                    _fluidPageContent)));
            }

            return new DirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (TryGetTemplatePath(subpath, out var path))
            {
                if (Templates.TryGetValue(path, out var template))
                {
                    if (Path.GetExtension(subpath) == FluidViewTemplate.ViewExtension)
                    {
                        return new ContentFileInfo(Path.GetFileName(path), template.Content);
                    }
                    else if (Path.GetExtension(subpath) == RazorViewEngine.ViewExtension)
                    {
                        return new ContentFileInfo(Path.GetFileName(subpath), _fluidPageContent);
                    }
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
                    return _templatesChangeToken;
                }
            }

            return null;
        }

        private IDictionary<string, Models.Template> GetTemplates()
        {
            if (_templatesChangeToken == null || _templatesChangeToken.HasChanged)
            {
                var templateManager = _httpContextAccessor.HttpContext.RequestServices.GetService<TemplatesManager>(); ;
                _templates = templateManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult().Templates;
                _templatesChangeToken = templateManager.ChangeToken;
            }

            return _templates;
        }

        private bool TryGetTemplatePath(string subpath, out string templatePath)
        {
            subpath = subpath.Replace("\\", "/").TrimStart('/');

            var key = _themesViewsPaths.Keys.FirstOrDefault(k => subpath.StartsWith(string.Format("{0}/", k)));

            if (key != null)
            {
                templatePath = Path.ChangeExtension(string.Format("{0}/{1}", _themesViewsPaths[key],
                    subpath.TrimStart('/').Substring(key.Length + 1)), FluidViewTemplate.ViewExtension);

                return true;
            }

            templatePath = null;
            return false;
        }
    }
}
