using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.Admin;
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

            if (TryGetTemplateFolderName(subpath, out var folderName))
            {
                entries.AddRange(Templates.Where(kv => kv.Key.StartsWith(folderName)).Select(kvp =>
                    new ContentFileInfo(kvp.Key.Substring(folderName.Length + 1), kvp.Value.Content)));
            }

            return new DirectoryContents(entries);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (TryGetTemplateFileName(subpath, out var fileName))
            {
                if (Templates.TryGetValue(fileName, out var template))
                {
                    return new ContentFileInfo(Path.GetFileName(fileName), template.Content);
                }
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (TryGetTemplateFileName(filter, out var fileName))
            {
                if (Templates.TryGetValue(fileName, out var template))
                {
                    return TemplatesManager.ChangeToken;
                }
            }

            return null;
        }

        private bool TryGetTemplateFolderName(string path, out string folderName)
        {
            var segments = path.TrimStart('/').Split('/');
            var index = Array.IndexOf(segments, "Views");

            if (index > 1 && index == segments.Count() - 1)
            {
                folderName = string.Format("{0}/{1}", segments[index - 1], segments[index]);
                return true;
            }

            folderName = null;
            return false;
        }

        private bool TryGetTemplateFileName(string path, out string fileName)
        {
            if (!AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                var segments = path.TrimStart('/').Split('/');
                var index = Array.IndexOf(segments, "Views");

                if (index > 1 && index < segments.Count() - 1)
                {
                    fileName = string.Format("{0}/{1}/{2}", segments[index - 1], segments[index],
                        string.Join("/", segments, index + 1, segments.Count() - index - 1));
                    return true;
                }
            }

            fileName = null;
            return false;
        }
    }
}
