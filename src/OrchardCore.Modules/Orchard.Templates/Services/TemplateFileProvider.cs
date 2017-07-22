using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement.FileProviders;

namespace Orchard.Templates.Services
{
    public interface ITemplateFileProvider : IFileProvider { }

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the template contents
    /// </summary>
    public class TemplateFileProvider : ITemplateFileProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TemplateFileProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private TemplatesManager TemplatesManager => _httpContextAccessor
            .HttpContext.RequestServices.GetRequiredService<TemplatesManager>();

        private Dictionary<string, Models.Template> Templates => TemplatesManager
            .GetTemplatesDocumentAsync().GetAwaiter().GetResult().Templates;

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var entries = new List<IFileInfo>();
            var name = Path.GetFileName(subpath);

            if (name == "Views")
            {
                var path = string.Format("{0}/{1}/", Path.GetFileName(Path.GetDirectoryName(subpath)), name);

                entries.AddRange(Templates.Where(kv => kv.Key.StartsWith(path)).Select(kvp =>
                    new ContentFileInfo(kvp.Key.Substring(path.Length), kvp.Value.Content)));
            }

            return new DirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var template = Templates.FirstOrDefault(kv => subpath.TrimStart('/').EndsWith(kv.Key));

            if (template.Key != null)
            {
                return new ContentFileInfo(Path.GetFileName(template.Key), template.Value.Content);
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            var template = Templates.FirstOrDefault(kv => filter.TrimStart('/').EndsWith(kv.Key));

            if (template.Key != null)
            {
                return TemplatesManager.ChangeToken;
            }

            return null;
        }
    }
}
