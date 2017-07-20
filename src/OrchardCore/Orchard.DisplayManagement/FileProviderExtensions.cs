using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace Orchard.DisplayManagement
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<string> GetViewFilePaths(this IFileProvider fileProvider, string subPath, string[] extensions, bool inViewsFolder = false, bool inDepth = true)
        {
            var contents = fileProvider.GetDirectoryContents(subPath);

            if (!inViewsFolder && inDepth)
            {
                var viewsFolder = contents.FirstOrDefault(x => x.Name == "Views" && x.IsDirectory);

                if (viewsFolder != null)
                {
                    foreach (var file in GetViewFilePaths(fileProvider, string.Format("{0}/{1}", subPath, viewsFolder.Name), extensions, true))
                    {
                        yield return file;
                    }

                    yield break;
                }
            }

            foreach (var content in contents)
            {
                if (content.IsDirectory && inDepth)
                {
                    foreach (var file in GetViewFilePaths(fileProvider, string.Format("{0}/{1}", subPath, content.Name), extensions, inViewsFolder))
                    {
                        yield return file;
                    }
                }
                else if (inViewsFolder)
                {
                    if (extensions.Contains(Path.GetExtension(content.Name)))
                    {
                        yield return string.Format("{0}/{1}", subPath, content.Name);
                    }
                }
            }
        }
    }
}
